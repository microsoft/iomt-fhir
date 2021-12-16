using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Validation;
using EnsureThat;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Tools.EventDebugger.EventProcessor
{
    public class DeviceEventProcessor
    {
        private const TaskContinuationOptions AsyncContinueOnSuccess = TaskContinuationOptions.RunContinuationsAsynchronously | TaskContinuationOptions.NotOnFaulted | TaskContinuationOptions.NotOnCanceled;
        private readonly ILogger<DeviceEventProcessor> _logger;
        private readonly IConverter<EventData, JToken> _converter = null;
        private readonly IIotConnectorValidator _iotConnectorValidator;
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
            IIotConnectorValidator iotConnectorValidator,
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

                        await foreach(var partitionEvent in _eventHubConsumerClient.ReadEventsAsync(readOptions, cancellationToken))
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
                  } catch (OperationCanceledException ex)
                  {
                      _logger.LogInformation(ex,"Timeout reached while reading from EventHub");
                  } finally
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

            var transformer = new TransformBlock<EventData, ValidationResult>(
                evt =>
                {
                    var validationResult = new ValidationResult();
                    
                    try
                    {
                        var token = _converter.Convert(evt);
                        validationResult.DeviceEvent = token;
                        validationResult = _iotConnectorValidator.PerformValidation(token, deviceMappingContent, fhirMappingContent);
                        validationResult.SequenceNumber = evt.SequenceNumber;
                    }
                    catch (Exception ex)
                    {
                        validationResult.Exceptions.Add(ex.Message);
                    }

                    if (validationResult.Exceptions.Count + validationResult.Warnings.Count == 0)
                    {
                        Interlocked.Increment(ref totalSuccessfulEvents);
                    }
                    else
                    {
                        Interlocked.Increment(ref totalFailedEvents);
                    }

                    return validationResult;
                },
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxParallelism, SingleProducerConstrained = true, CancellationToken = cancellationToken });

            var consumer = new ActionBlock<ValidationResult>(
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
    }
}