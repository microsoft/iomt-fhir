// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using EnsureThat;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Validation;
using Microsoft.Health.Fhir.Ingest.Validation.Extensions;
using Microsoft.Health.Fhir.Ingest.Validation.Models;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Tools.EventDebugger.EventProcessor
{
    public class DeviceEventProcessor
    {
        private const TaskContinuationOptions AsyncContinueOnSuccess = TaskContinuationOptions.RunContinuationsAsynchronously | TaskContinuationOptions.NotOnFaulted | TaskContinuationOptions.NotOnCanceled;
        private readonly ILogger<DeviceEventProcessor> _logger;
        private readonly IConverter<EventData, JToken> _converter = null;
        private readonly IMappingValidator _iotConnectorValidator;
        private readonly IConversionResultWriter _conversionResultWriter;
        private EventHubConsumerClient _eventHubConsumerClient;
        private int _maxParallelism = 4;
        private int _eventsToProcess;
        private TimeSpan _eventReadTimeout;

        private int totalEventsReceived;
        private int totalSuccessfulEvents;
        private int totalFailedEvents;

        public DeviceEventProcessor(
            ILogger<DeviceEventProcessor> logger,
            IConverter<EventData, JToken> converter,
            IMappingValidator iotConnectorValidator,
            IConversionResultWriter conversionResultWriter)
        {
            _logger = EnsureArg.IsNotNull(logger, nameof(logger));
            _converter = EnsureArg.IsNotNull(converter, nameof(converter));
            _iotConnectorValidator = EnsureArg.IsNotNull(iotConnectorValidator, nameof(iotConnectorValidator));
            _conversionResultWriter = EnsureArg.IsNotNull(conversionResultWriter, nameof(conversionResultWriter));
        }

        public async Task RunAsync(
            ValidationOptions validationOptions,
            EventProcessorOptions eventProcessorOptions,
            EventHubConsumerClient eventHubConsumerClient,
            CancellationToken cancellationToken)
        {
            _eventHubConsumerClient = EnsureArg.IsNotNull(eventHubConsumerClient, nameof(eventHubConsumerClient));

            var options = EnsureArg.IsNotNull(eventProcessorOptions, nameof(eventProcessorOptions));
            _eventReadTimeout = options.EventReadTimeout;
            _eventsToProcess = eventProcessorOptions.TotalEventsToProcess;

            await StartConsumer(StartProducer(eventProcessorOptions, cancellationToken), validationOptions, cancellationToken).ConfigureAwait(false);
        }

        private ISourceBlock<EventData> StartProducer(EventProcessorOptions eventProcessorOptions, CancellationToken cancellationToken)
        {
            var producer = new BufferBlock<EventData>(new DataflowBlockOptions { BoundedCapacity = DataflowBlockOptions.Unbounded });

            _ = Task.Run(async () =>
              {
                  try
                  {
                      var readOptions = new ReadEventOptions()
                      {
                          MaximumWaitTime = _eventReadTimeout,
                      };

                      if (eventProcessorOptions.EnqueuedTime != default)
                      {
                          _logger.LogInformation($"Reading new messages enqueued since: {eventProcessorOptions.EnqueuedTime} (Local Time).");
                      }

                      var eventPosition = eventProcessorOptions.EnqueuedTime == default ? EventPosition.Earliest : EventPosition.FromEnqueuedTime(new DateTimeOffset(eventProcessorOptions.EnqueuedTime));

                      await ReadEventsFromAllPartitions(producer, eventPosition, readOptions, cancellationToken);
                  }
                  catch (OperationCanceledException ex)
                  {
                      _logger.LogInformation(ex, "Timeout reached while reading from EventHub");
                  }
                  catch (FormatException e)
                  {
                      _logger.LogInformation(e, "Improperly formatted date");
                  }
                  finally
                  {
                      producer.Complete();
                  }
              });

            return producer;
        }

        private async Task StartConsumer(ISourceBlock<EventData> producer, ValidationOptions validationOptions, CancellationToken cancellationToken)
        {
            var deviceMappingContent = await File.ReadAllTextAsync(validationOptions.DeviceMapping.FullName, cancellationToken);
            var fhirMappingContent = string.Empty;

            if (validationOptions.FhirMapping != null)
            {
                fhirMappingContent = await File.ReadAllTextAsync(validationOptions.FhirMapping.FullName, cancellationToken);
            }

            var transformer = new TransformBlock<EventData, DebugValidationResult>(
                evt =>
                {
                    var debugResults = new DebugValidationResult();

                    try
                    {
                        var token = _converter.Convert(evt);
                        debugResults.ValidationResult = _iotConnectorValidator.PerformValidation(token, deviceMappingContent, fhirMappingContent);
                        debugResults.SequenceNumber = evt.SequenceNumber;
                    }
                    catch (Exception ex)
                    {
                        debugResults.ValidationResult.TemplateResult.CaptureException(ex);
                    }

                    if (debugResults.ValidationResult.AnyException(ErrorLevel.ERROR) || debugResults.ValidationResult.AnyException(ErrorLevel.WARN))
                    {
                        Interlocked.Increment(ref totalFailedEvents);
                    }
                    else
                    {
                        Interlocked.Increment(ref totalSuccessfulEvents);
                    }

                    return debugResults;
                },
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxParallelism, SingleProducerConstrained = true, CancellationToken = cancellationToken });

            var consumer = new ActionBlock<DebugValidationResult>(
                async result =>
                {
                    await _conversionResultWriter.StoreConversionResult(result, cancellationToken);
                },
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxParallelism, SingleProducerConstrained = true, CancellationToken = cancellationToken });

            _ = producer.LinkTo(transformer, new DataflowLinkOptions { PropagateCompletion = true });
            transformer.LinkTo(consumer, new DataflowLinkOptions { PropagateCompletion = true });

            await consumer.Completion
                .ContinueWith(
                    task =>
                    {
                        _logger.LogInformation($"Finished processing {totalFailedEvents + totalSuccessfulEvents} device events. Events with errors: {totalFailedEvents}");
                    },
                    cancellationToken,
                    AsyncContinueOnSuccess,
                    TaskScheduler.Current)
                .ConfigureAwait(false);
        }

        private async Task ReadEventsFromAllPartitions(
            ITargetBlock<EventData> producer,
            EventPosition eventPosition,
            ReadEventOptions readEventOptions,
            CancellationToken cancellationToken)
        {
            var ids = await _eventHubConsumerClient.GetPartitionIdsAsync(cancellationToken);
            var partitionReaderTasks = new List<Task>();

            foreach (var id in ids)
            {
                var partitionReadTask = Task.Run(async () =>
                    {
                        try
                        {
                            await foreach (var partitionEvent in _eventHubConsumerClient.ReadEventsFromPartitionAsync(id, eventPosition, readEventOptions, cancellationToken))
                            {
                                var currentCount = Interlocked.Increment(ref totalEventsReceived);

                                if (partitionEvent.Data == null)
                                {
                                    // Azure EventHubs has sent us an empty message indicating no new events to process within the _eventReadTimeout limit, stop reading new events
                                    _logger.LogTrace($"No new events on EventHub Partition {id} within {_eventReadTimeout.TotalSeconds} seconds. Ending read operation.");
                                    break;
                                }

                                if (currentCount % 10 == 0)
                                {
                                    _logger.LogInformation($"Received {currentCount} events");
                                }

                                if (currentCount > _eventsToProcess)
                                {
                                    break;
                                }

                                while (!await producer.SendAsync(partitionEvent.Data))
                                {
                                    await Task.Yield();
                                }
                            }
                        }
                        catch (OperationCanceledException ex)
                        {
                            _logger.LogInformation(ex, $"Timeout reached while reading from EventHub on Partition {id}");
                        }
                    });

                partitionReaderTasks.Add(partitionReadTask);
            }

            Task.WaitAll(partitionReaderTasks.ToArray(), cancellationToken);
        }
    }
}