using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Validation;
using Microsoft.Health.Fhir.Ingest.Validation.Extensions;
using Microsoft.Health.Fhir.Ingest.Validation.Models;
using EnsureThat;
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

            await StartConsumer(StartProducer(cancellationToken), validationOptions, cancellationToken).ConfigureAwait(false);
        }

        private ISourceBlock<EventData> StartProducer(CancellationToken cancellationToken)
        {
            var producer = new BufferBlock<EventData>(new DataflowBlockOptions { BoundedCapacity = DataflowBlockOptions.Unbounded });

            _ = Task.Run(async () =>
              {
                  int count = 0;
                  try
                  {
                      var readOptions = new ReadEventOptions()
                      {
                          MaximumWaitTime = _eventReadTimeout,
                      };

                      var eventPosition = EventPosition.Earliest;

                      await foreach (var partitionEvent in await ReadEventsFromAllPartitions(eventPosition, readOptions, cancellationToken))
                      {
                          // TODO Filter out messages here... Perhaps with JmesPath Expression?
                          if (partitionEvent.Data == null)
                          {
                              // Azure EventHubs has sent us an empty message indicating no new events to process within the _eventReadTimeout limit, end program
                              _logger.LogInformation($"No new events on EventHub within {_eventReadTimeout.TotalSeconds} seconds. Ending program");
                              break;
                          }

                          if (++count % 10 == 0)
                          {
                              _logger.LogInformation($"Received {count} events");
                          }

                          if (count > _eventsToProcess)
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
                      _logger.LogInformation(ex, "Timeout reached while reading from EventHub");
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

            var transformer = new TransformBlock<EventData, DebugResult>(
                evt =>
                {
                    var debugResults = new DebugResult();

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

            var consumer = new ActionBlock<DebugResult>(
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

        private async Task<IAsyncEnumerable<PartitionEvent>> ReadEventsFromAllPartitions(
            EventPosition eventPosition,
            ReadEventOptions readEventOptions,
            CancellationToken cancellationToken)
        {
            var ids = await _eventHubConsumerClient.GetPartitionIdsAsync(cancellationToken);

            return ids.Select(id => _eventHubConsumerClient.ReadEventsFromPartitionAsync(id, eventPosition, readEventOptions, cancellationToken))
                .Aggregate((left, right) => left.Concat(right));
        }
    }
}