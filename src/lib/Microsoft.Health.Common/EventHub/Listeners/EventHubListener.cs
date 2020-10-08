// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Scale;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Common.EventHubs.Listeners;
using Newtonsoft.Json;

namespace Microsoft.Health.Common.EventHubs
{
    internal sealed class EventHubListener : IListener, IEventProcessorFactory, IScaleMonitorProvider
    {
        private static readonly Dictionary<string, object> EmptyScope = new Dictionary<string, object>();
        private readonly string _functionId;
        private readonly string _eventHubName;
        private readonly string _consumerGroup;
        private readonly string _connectionString;
        private readonly string _storageConnectionString;
        private readonly ITriggeredFunctionExecutor _executor;
        private readonly EventProcessorHost _eventProcessorHost;
        private readonly bool _singleDispatch;
        private readonly EventHubOptions _options;
        private readonly ILogger _logger;
        private bool _started;

        private Lazy<EventHubsScaleMonitor> _scaleMonitor;

        public EventHubListener(
            string functionId,
            string eventHubName,
            string consumerGroup,
            string connectionString,
            string storageConnectionString,
            ITriggeredFunctionExecutor executor,
            EventProcessorHost eventProcessorHost,
            bool singleDispatch,
            EventHubOptions options,
            ILogger logger,
            CloudBlobContainer blobContainer = null)
        {
            _functionId = functionId;
            _eventHubName = eventHubName;
            _consumerGroup = consumerGroup;
            _connectionString = connectionString;
            _storageConnectionString = storageConnectionString;
            _executor = executor;
            _eventProcessorHost = eventProcessorHost;
            _singleDispatch = singleDispatch;
            _options = options;
            _logger = logger;
            _scaleMonitor = new Lazy<EventHubsScaleMonitor>(() => new EventHubsScaleMonitor(_functionId, _eventHubName, _consumerGroup, _connectionString, _storageConnectionString, _logger, blobContainer));
        }

        void IListener.Cancel()
        {
            StopAsync(CancellationToken.None).Wait();
        }

        void IDisposable.Dispose()
        {
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _eventProcessorHost.RegisterEventProcessorFactoryAsync(this, _options.EventProcessorOptions);
            _started = true;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_started)
            {
                await _eventProcessorHost.UnregisterEventProcessorAsync();
            }

            _started = false;
        }

        IEventProcessor IEventProcessorFactory.CreateEventProcessor(PartitionContext context)
        {
            return new EventProcessor(_options, _executor, _logger, _singleDispatch);
        }

        public IScaleMonitor GetMonitor()
        {
            return _scaleMonitor.Value;
        }

        /// <summary>
        /// Wrapper for un-mockable checkpoint APIs to aid in unit testing
        /// </summary>
#pragma warning disable SA1201 // Elements should appear in the correct order
        internal interface ICheckpointer
#pragma warning restore SA1201 // Elements should appear in the correct order
        {
            Task CheckpointAsync(PartitionContext context);
        }

        // We get a new instance each time Start() is called.
        // We'll get a listener per partition - so they can potentialy run in parallel even on a single machine.

#pragma warning disable CA1303 // Do not pass literals as localized parameters
        internal class EventProcessor : IEventProcessor, IDisposable, ICheckpointer
        {
            private readonly ITriggeredFunctionExecutor _executor;
            private readonly bool _singleDispatch;
            private readonly ILogger _logger;
            private readonly CancellationTokenSource _cts = new CancellationTokenSource();
            private readonly ICheckpointer _checkpointer;
            private readonly int _batchCheckpointFrequency;
            private readonly int _eventQueueSize;
            private readonly TimeSpan _flushTimeSpan;
            private readonly ConcurrentDictionary<string, PartitionEventData> _pendingEventData;
            private int _batchCounter = 0;
            private bool _disposed = false;
            private DateTime _firstEnqueuedDateTime;
            private DateTime _currentIntervalStart;
            private DateTime _currentIntervalEnd;
            private readonly Task _timeIntervalFlush;

            public EventProcessor(EventHubOptions options, ITriggeredFunctionExecutor executor, ILogger logger, bool singleDispatch, ICheckpointer checkpointer = null)
            {
                _checkpointer = checkpointer ?? this;
                _executor = executor;
                _singleDispatch = singleDispatch;
                _batchCheckpointFrequency = options.BatchCheckpointFrequency;
                _logger = logger;
                _pendingEventData = new ConcurrentDictionary<string, PartitionEventData>();
                _flushTimeSpan = TimeSpan.FromMinutes(1);
                _eventQueueSize = options.EventQueueSize;
                _firstEnqueuedDateTime = DateTime.MinValue;
                _currentIntervalStart = DateTime.MinValue;
                _currentIntervalEnd = DateTime.MinValue;
                _timeIntervalFlush = Task.Run(TimeIntervalFlush);
            }

            public Task CloseAsync(PartitionContext context, CloseReason reason)
            {
                // signal cancellation for any in progress executions
                _cts.Cancel();

                _logger.LogDebug(GetOperationDetails(context, $"CloseAsync, {reason}"));
                return Task.CompletedTask;
            }

            public Task OpenAsync(PartitionContext context)
            {
                _logger.LogDebug(GetOperationDetails(context, "OpenAsync"));
                return Task.CompletedTask;
            }

            public Task ProcessErrorAsync(PartitionContext context, Exception error)
            {
                string errorDetails = $"Partition Id: '{context.PartitionId}', Owner: '{context.Owner}', EventHubPath: '{context.EventHubPath}'";

                if (error is ReceiverDisconnectedException ||
                    error is LeaseLostException)
                {
                    // For EventProcessorHost these exceptions can happen as part
                    // of normal partition balancing across instances, so we want to
                    // trace them, but not treat them as errors.
                    _logger.LogInformation($"An Event Hub exception of type '{error.GetType().Name}' was thrown from {errorDetails}. This exception type is typically a result of Event Hub processor rebalancing and can be safely ignored.");
                }
                else
                {
                    _logger.LogError(error, $"Error processing event from {errorDetails}");
                }

                return Task.CompletedTask;
            }

            public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
            {
                // Queue events as they come in. Track according to partition. Keep last known partition context with queued messages.
                // Current implementation doesn't taken into account queued time.  A more advanced implementation would batch events
                // according to how they slot according to the flush time period.  I.E. we receive 2000 events spread across 3 minutes
                // we would send them according to they slot into 3 minute periods for at least 3 batches (possibly more if one period
                // contained more than 1000 events.  Using queued time would match what SA does and allow for deterministic replay of
                // the pipeline.

                var events = _pendingEventData.AddOrUpdate(
                    context.PartitionId,
                    p => new PartitionEventData(context, messages),
                    (p, d) =>
                    {
                        d.Context = context;
                        foreach (var message in messages)
                        {
                            d.Data.Enqueue(message);
                        }

                        return d;
                    });

                // Immediately process events queued up if we have reached our queue size
                if (events.Data.Count >= _eventQueueSize)
                {
                    await ConsumeEventsAsync();
                }
            }

            private async Task TimedFlush()
            {
                while (!_cts.IsCancellationRequested)
                {
                    DateTimeOffset nextReading = DateTimeOffset.UtcNow.Add(_flushTimeSpan);

                    var delayTime = nextReading - DateTimeOffset.UtcNow;
                    if (delayTime.TotalMilliseconds > 0)
                    {
                        await Task.Delay(delayTime);
                    }

                    _logger.LogInformation("Timed flush triggered.");

                    // Simple logic for now.  Timer isn't reset if batch flush is triggered since last timed flush.
                    if (!_cts.IsCancellationRequested && !_pendingEventData.IsEmpty)
                    {
                        _logger.LogInformation("Events queued, timed flush triggered.");
                        await ConsumeEventsAsync();
                    }
                }
            }

            private async void TimeIntervalFlush()
            {
                while (!_cts.IsCancellationRequested)
                {
                    if (_currentIntervalEnd == DateTime.MinValue)
                    {
                        _currentIntervalEnd = CalculateIntervalEnd(_currentIntervalStart, _flushTimeSpan);
                    }

                    if (_currentIntervalEnd == DateTime.MinValue)
                    {
                        Console.WriteLine("Waiting for an event to establish a time interval");
                        await Task.Delay(10000);
                        continue;
                    }

                    // Wait until interval end is some time in the past
                    while (_currentIntervalEnd > DateTime.UtcNow.AddSeconds(-5))
                    {
                        Console.WriteLine($"Current UTC: {DateTime.UtcNow}");
                        Console.WriteLine($"Current Interval End: {_currentIntervalEnd} is greater than {DateTime.UtcNow.AddSeconds(-5)}");
                        Console.WriteLine("Waiting...");
                        await Task.Delay(5000);
                    }

                    // Process events within interval
                    await ConsumeEventsInIntervalAsync();
                    _currentIntervalStart = _currentIntervalStart.Add(_flushTimeSpan);
                    _currentIntervalEnd = _currentIntervalEnd.Add(_flushTimeSpan);
                    Console.WriteLine("Next Interval End: " + _currentIntervalEnd);
                }
            }

            private DateTime GetFirstEnqueuedTime()
            {
                // TODO - compare across all _pendingEventData.Values?
                foreach (var pendingData in _pendingEventData.Values)
                {
                    if (pendingData.Data.IsEmpty)
                    {
                        continue;
                    }

                    pendingData.Data.TryPeek(out var eventData);
                    eventData.SystemProperties.TryGetValue("x-opt-enqueued-time", out var enqueuedTime);
                    var enqueuedDateTime = Convert.ToDateTime(enqueuedTime);
                    eventData.Dispose();
                    return enqueuedDateTime;
                }

                // Else return min value
                return DateTime.MinValue;
            }

            private DateTime CalculateIntervalEnd(DateTime intervalStart, TimeSpan intervalLength)
            {
                if (intervalStart == DateTime.MinValue)
                {
                    intervalStart = GetFirstEnqueuedTime();
                }

                if (intervalStart == DateTime.MinValue)
                {
                    return intervalStart;
                }

                var intervalEnd = intervalStart.Add(intervalLength);

                return intervalEnd;
            }

            // Consume events within a certain interval
            private async Task ConsumeEventsInIntervalAsync()
            {
                var intervalEnd = _currentIntervalEnd;

                Console.WriteLine($"=======Flushing Events up to: {intervalEnd}=======");

                // find index, then dequeue up to index.
                foreach (var pendingData in _pendingEventData.Values)
                {
                    if (pendingData.Data.IsEmpty)
                    {
                        continue;
                    }

                    var batch = new List<EventData>();
                    while (pendingData.Data.TryPeek(out var eventData))
                    {
                        eventData.SystemProperties.TryGetValue("x-opt-enqueued-time", out var enqueuedTime);
                        var enqueuedDateTime = Convert.ToDateTime(enqueuedTime);
                        eventData.Dispose();

                        // Stop if enqueuedDateTime greather than intervalEnd
                        if (enqueuedDateTime > intervalEnd)
                        {
                            Console.WriteLine($"Enqueued Time: {enqueuedDateTime} greater than interval end time {intervalEnd}");
                            break;
                        }
                        else
                        {
                            pendingData.Data.TryDequeue(out var dequeuedEvent);
                            Console.WriteLine($"Flushing event with enqueued time: {enqueuedDateTime} less than {intervalEnd}");
                            batch.Add(dequeuedEvent);
                        }
                    }

                    // flush it all
                    if (batch.Count > 0)
                    {
                        EventHubTriggerInput triggerInput = new EventHubTriggerInput
                        {
                            PartitionContext = pendingData.Context,
                            Events = batch.ToArray(),
                        };

                        Console.WriteLine($"Flushing events: {batch.Count}");

                        await ConsumeEventsAsync(triggerInput).ConfigureAwait(false);
                    }
                }
            }

            private async Task ConsumeEventsAsync()
            {
                foreach (var pendingData in _pendingEventData.Values)
                {
                    if (pendingData.Data.IsEmpty)
                    {
                        continue;
                    }

                    var batch = new List<EventData>();

                    while (pendingData.Data.TryDequeue(out var eventData))
                    {
                        batch.Add(eventData);
                    }

                    EventHubTriggerInput triggerInput = new EventHubTriggerInput
                    {
                        PartitionContext = pendingData.Context,
                        Events = batch.ToArray(),
                    };

                    await ConsumeEventsAsync(triggerInput).ConfigureAwait(false);
                }
            }

            private async Task ConsumeEventsAsync(EventHubTriggerInput triggerInput)
            {
                TriggeredFunctionData input;
                if (_singleDispatch)
                {
                    // Single dispatch
                    int eventCount = triggerInput.Events.Length;
                    List<Task> invocationTasks = new List<Task>();
                    for (int i = 0; i < eventCount; i++)
                    {
                        if (_cts.IsCancellationRequested)
                        {
                            break;
                        }

                        EventHubTriggerInput eventHubTriggerInput = triggerInput.GetSingleEventTriggerInput(i);
                        input = new TriggeredFunctionData
                        {
                            TriggerValue = eventHubTriggerInput,
                            TriggerDetails = eventHubTriggerInput.GetTriggerDetails(triggerInput.PartitionContext),
                        };

                        Task task = TryExecuteWithLoggingAsync(input, triggerInput.Events[i]);
                        invocationTasks.Add(task);
                    }

                    // Drain the whole batch before taking more work
                    if (invocationTasks.Count > 0)
                    {
                        await Task.WhenAll(invocationTasks);
                    }
                }
                else
                {
                    // Batch dispatch
                    input = new TriggeredFunctionData
                    {
                        TriggerValue = triggerInput,
                        TriggerDetails = triggerInput.GetTriggerDetails(triggerInput.PartitionContext),
                    };

                    using (_logger.BeginScope(GetLinksScope(triggerInput.Events)))
                    {
                        await _executor.TryExecuteAsync(input, _cts.Token);
                    }
                }

                // Dispose all messages to help with memory pressure. If this is missed, the finalizer thread will still get them.
                bool hasEvents = false;
                foreach (var message in triggerInput.Events)
                {
                    hasEvents = true;
                    message.Dispose();
                }

                // Checkpoint if we processed any events.
                // Don't checkpoint if no events. This can reset the sequence counter to 0.
                // Note: we intentionally checkpoint the batch regardless of function
                // success/failure. EventHub doesn't support any sort "poison event" model,
                // so that is the responsibility of the user's function currently. E.g.
                // the function should have try/catch handling around all event processing
                // code, and capture/log/persist failed events, since they won't be retried.
                if (hasEvents)
                {
                    await CheckpointAsync(triggerInput.PartitionContext);
                }
            }

            private async Task TryExecuteWithLoggingAsync(TriggeredFunctionData input, EventData message)
            {
                using (_logger.BeginScope(GetLinksScope(message)))
                {
                    await _executor.TryExecuteAsync(input, _cts.Token);
                }
            }

            private async Task CheckpointAsync(PartitionContext context)
            {
                bool checkpointed = false;
                if (_batchCheckpointFrequency == 1)
                {
                    await _checkpointer.CheckpointAsync(context);
                    checkpointed = true;
                }
                else
                {
                    // only checkpoint every N batches
                    if (++_batchCounter >= _batchCheckpointFrequency)
                    {
                        _batchCounter = 0;
                        await _checkpointer.CheckpointAsync(context);
                        checkpointed = true;
                    }
                }

                if (checkpointed)
                {
                    _logger.LogDebug(GetOperationDetails(context, "CheckpointAsync"));
                }
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!_disposed)
                {
                    if (disposing)
                    {
                        _cts.Dispose();
                    }

                    _disposed = true;
                }
            }

            public void Dispose()
            {
                Dispose(true);
            }

            async Task ICheckpointer.CheckpointAsync(PartitionContext context)
            {
                await context.CheckpointAsync();
            }

            private static Dictionary<string, object> GetLinksScope(EventData message)
            {
                if (TryGetLinkedActivity(message, out var link))
                {
                    return new Dictionary<string, object> { ["Links"] = new[] { link } };
                }

                return EmptyScope;
            }

            private static Dictionary<string, object> GetLinksScope(EventData[] messages)
            {
                List<Activity> links = null;

                foreach (var message in messages)
                {
                    if (TryGetLinkedActivity(message, out var link))
                    {
                        if (links == null)
                        {
                            links = new List<Activity>(messages.Length);
                        }

                        links.Add(link);
                    }
                }

                if (links != null)
                {
                    return new Dictionary<string, object> { ["Links"] = links };
                }

                return EmptyScope;
            }

            private static bool TryGetLinkedActivity(EventData message, out Activity link)
            {
                link = null;

                if (((message.SystemProperties != null && message.SystemProperties.TryGetValue("Diagnostic-Id", out var diagnosticIdObj)) || message.Properties.TryGetValue("Diagnostic-Id", out diagnosticIdObj))
                    && diagnosticIdObj is string diagnosticIdString)
                {
                    link = new Activity("Microsoft.Azure.EventHubs.Process");
                    link.SetParentId(diagnosticIdString);
                    return true;
                }

                return false;
            }

            private static string GetOperationDetails(PartitionContext context, string operation)
            {
                StringWriter sw = new StringWriter();
                using (JsonTextWriter writer = new JsonTextWriter(sw) { Formatting = Formatting.None })
                {
                    writer.WriteStartObject();
                    WritePropertyIfNotNull(writer, "operation", operation);
                    writer.WritePropertyName("partitionContext");
                    writer.WriteStartObject();
                    WritePropertyIfNotNull(writer, "partitionId", context.PartitionId);
                    WritePropertyIfNotNull(writer, "owner", context.Owner);
                    WritePropertyIfNotNull(writer, "eventHubPath", context.EventHubPath);
                    writer.WriteEndObject();

                    // Log partition lease
                    if (context.Lease != null)
                    {
                        writer.WritePropertyName("lease");
                        writer.WriteStartObject();
                        WritePropertyIfNotNull(writer, "offset", context.Lease.Offset);
                        WritePropertyIfNotNull(writer, "sequenceNumber", context.Lease.SequenceNumber.ToString());
                        writer.WriteEndObject();
                    }

                    // Log RuntimeInformation if EnableReceiverRuntimeMetric is enabled
                    if (context.RuntimeInformation != null)
                    {
                        writer.WritePropertyName("runtimeInformation");
                        writer.WriteStartObject();
                        WritePropertyIfNotNull(writer, "lastEnqueuedOffset", context.RuntimeInformation.LastEnqueuedOffset);
                        WritePropertyIfNotNull(writer, "lastSequenceNumber", context.RuntimeInformation.LastSequenceNumber.ToString());
                        WritePropertyIfNotNull(writer, "lastEnqueuedTimeUtc", context.RuntimeInformation.LastEnqueuedTimeUtc.ToString("o"));
                        writer.WriteEndObject();
                    }

                    writer.WriteEndObject();
                }

                return sw.ToString();
            }

            private static void WritePropertyIfNotNull(JsonTextWriter writer, string propertyName, string propertyValue)
            {
                if (propertyValue != null)
                {
                    writer.WritePropertyName(propertyName);
                    writer.WriteValue(propertyValue);
                }
            }

            private class PartitionEventData
            {
                public PartitionEventData(PartitionContext partitionContext, IEnumerable<EventData> data)
                {
                    Context = partitionContext;
                    Data = new ConcurrentQueue<EventData>(data);
                }

                public PartitionContext Context { get; set; }

                public ConcurrentQueue<EventData> Data { get; }
            }
        }
#pragma warning restore CA1303 // Do not pass literals as localized parameters
    }
}