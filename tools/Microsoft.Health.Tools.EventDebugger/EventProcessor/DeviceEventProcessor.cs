using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Template;
using EnsureThat;
using Newtonsoft.Json.Linq;
using Microsoft.Health.Tools.EventDebugger.TemplateLoader;

namespace Microsoft.Health.Tools.EventDebugger.EventProcessor
{
    public class DeviceEventProcessor
    {
        private const TaskContinuationOptions AsyncContinueOnSuccess = TaskContinuationOptions.RunContinuationsAsynchronously | TaskContinuationOptions.NotOnFaulted | TaskContinuationOptions.NotOnCanceled;
        private readonly EventHubConsumerClient _eventHubConsumerClient;
        private readonly TimeSpan _eventReadTimeout;
        private readonly ILogger<DeviceEventProcessor> _logger;
        private readonly IConverter<EventData, JToken> _converter = null;
        private readonly ITemplateLoader _templateLoader;
        private readonly IConversionResultWriter _conversionResultWriter;
        private int _maxParallelism = 4;
        private int _eventsToProcess;

        private int totalSuccessfulEvents;
        private int totalFailedEvents;

        public DeviceEventProcessor(
            EventHubConsumerClient eventHubConsumerClient,
            ILogger<DeviceEventProcessor> logger,
            IConverter<EventData, JToken> converter,
            ITemplateLoader templateLoader,
            IConversionResultWriter conversionResultWriter,
            IOptions<EventProcessorOptions> eventProcessorOptions)
        {
            _eventHubConsumerClient = EnsureArg.IsNotNull(eventHubConsumerClient, nameof(eventHubConsumerClient));
            _logger = EnsureArg.IsNotNull(logger, nameof(logger));
            _converter = EnsureArg.IsNotNull(converter, nameof(converter));
            _templateLoader = EnsureArg.IsNotNull(templateLoader, nameof(templateLoader));
            _conversionResultWriter = EnsureArg.IsNotNull(conversionResultWriter, nameof(conversionResultWriter));
            
            var options = EnsureArg.IsNotNull(eventProcessorOptions.Value, nameof(eventProcessorOptions));
            _eventReadTimeout = options.EventReadTimeout;
            _eventsToProcess = eventProcessorOptions.Value.TotalEventsToProcess;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            var template = await _templateLoader.LoadTemplate();

            await StartConsumer(StartProducer(cancellationToken), template, cancellationToken).ConfigureAwait(false);
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
                            // TODO Filter out messages here...
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

        private async Task StartConsumer(ISourceBlock<EventData> producer, IContentTemplate contentTemplate, CancellationToken cancellationToken)
        {
            var transformer = new TransformBlock<EventData, ConversionResult>(
                evt =>
                {
                    var conversionResult = new ConversionResult();
                    conversionResult.SequenceNumber = evt.SequenceNumber;
                    try
                    {
                        var token = _converter.Convert(evt);
                        conversionResult.DeviceEvent = token;

                        foreach (var measurement in contentTemplate.GetMeasurements(token))
                        {
                            // TODO If we only want errors then do not store the Measurement
                            measurement.IngestionTimeUtc = evt.EnqueuedTime.DateTime;
                            conversionResult.Measurements.Add(measurement);
                            Interlocked.Increment(ref totalSuccessfulEvents);
                        }
                    }
                    catch (Exception ex)
                    {
                        conversionResult.Exceptions.Add(ex);
                        Interlocked.Increment(ref totalFailedEvents);
                    }

                    return conversionResult;
                },
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxParallelism, SingleProducerConstrained = true, CancellationToken = cancellationToken });

            var consumer = new ActionBlock<ConversionResult>(
                async result =>
                {
                    // Do cool stuff with the result. 
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