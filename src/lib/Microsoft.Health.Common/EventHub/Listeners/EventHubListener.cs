// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        internal class EventProcessor : IEventProcessor, IDisposable, ICheckpointer
        {
            private readonly ITriggeredFunctionExecutor _executor;
            private readonly bool _singleDispatch;
            private readonly ILogger _logger;
            private readonly CancellationTokenSource _cts = new CancellationTokenSource();
            private readonly ICheckpointer _checkpointer;
            private readonly int _batchCheckpointFrequency;
            private int _batchCounter = 0;
            private bool _disposed = false;

            public EventProcessor(EventHubOptions options, ITriggeredFunctionExecutor executor, ILogger logger, bool singleDispatch, ICheckpointer checkpointer = null)
            {
                _checkpointer = checkpointer ?? this;
                _executor = executor;
                _singleDispatch = singleDispatch;
                _batchCheckpointFrequency = options.BatchCheckpointFrequency;
                _logger = logger;
            }

            public Task CloseAsync(PartitionContext context, CloseReason reason)
            {
                // signal cancellation for any in progress executions
                _cts.Cancel();

                _logger.LogDebug(GetOperationDetails(context, $"CloseAsync, {reason.ToString()}"));
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
                var triggerInput = new EventHubTriggerInput
                {
                    Events = messages.ToArray(),
                    PartitionContext = context,
                };

                TriggeredFunctionData input = null;
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
                            TriggerDetails = eventHubTriggerInput.GetTriggerDetails(context),
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
                        TriggerDetails = triggerInput.GetTriggerDetails(context),
                    };

                    using (_logger.BeginScope(GetLinksScope(triggerInput.Events)))
                    {
                        await _executor.TryExecuteAsync(input, _cts.Token);
                    }
                }

                // Dispose all messages to help with memory pressure. If this is missed, the finalizer thread will still get them.
                bool hasEvents = false;
                foreach (var message in messages)
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
                    await CheckpointAsync(context);
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

            private Dictionary<string, object> GetLinksScope(EventData[] messages)
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
        }
    }
}