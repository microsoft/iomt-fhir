using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using Microsoft.Extensions.Logging;
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
        private int _maxParallelism = 4;

        public DeviceEventProcessor(
            EventHubConsumerClient eventHubConsumerClient,
            ILogger<DeviceEventProcessor> logger,
            IConverter<EventData, JToken> converter,
            ITemplateLoader templateLoader,
            TimeSpan maximumTimeToReadEvents)
        {
            _eventHubConsumerClient = EnsureArg.IsNotNull(eventHubConsumerClient, nameof(eventHubConsumerClient));
            _logger = EnsureArg.IsNotNull(logger, nameof(logger));
            _eventReadTimeout = maximumTimeToReadEvents;
            _converter = EnsureArg.IsNotNull(converter, nameof(converter));
            _templateLoader = EnsureArg.IsNotNull(templateLoader, nameof(templateLoader));
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
                  await foreach(var partitionEvent in _eventHubConsumerClient.ReadEventsAsync(cancellationToken))
                  {
                      // TODO Filter out messages here...

                      // Determine if we should stop processing events. For now, hard code this to just process 5 messages
                      if (++count > 5)
                      {
                        break;
                      }

                      // Do stuff with the message
                      _logger.LogInformation($"Received a message from partition {partitionEvent.Partition.PartitionId}. Enqueued time: {partitionEvent.Data.EnqueuedTime}, Sequence Id: {partitionEvent.Data.SequenceNumber}");

                      while (!await producer.SendAsync(partitionEvent.Data))
                      {
                          await Task.Yield();
                      }
                  }

                  producer.Complete();
              });

            return producer;
        }

        private async Task StartConsumer(ISourceBlock<EventData> producer, IContentTemplate contentTemplate, CancellationToken cancellationToken)
        {
            var transformer = new TransformBlock<EventData, ConversionResult>(
                evt =>
                {
                    var conversionResult = new ConversionResult();
                    try
                    {
                        var token = _converter.Convert(evt);
                        conversionResult.DeviceEvent = token;

                        foreach (var measurement in contentTemplate.GetMeasurements(token))
                        {
                            // TODO If we only want errors then do not store the Measurement
                            measurement.IngestionTimeUtc = evt.EnqueuedTime.DateTime;
                            conversionResult.Measurements.Add(measurement);
                        }
                    }
                    catch (Exception ex)
                    {
                        conversionResult.Exceptions.Add(ex);
                    }

                    return conversionResult;
                },
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxParallelism, SingleProducerConstrained = true, CancellationToken = cancellationToken });

            var consumer = new ActionBlock<ConversionResult>(
                result =>
                {
                    // Do cool stuff with the result. 
                    _logger.LogInformation(result.DeviceEvent.ToString());
                },
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxParallelism, SingleProducerConstrained = true, CancellationToken = cancellationToken });

            _ = producer.LinkTo(transformer, new DataflowLinkOptions { PropagateCompletion = true });
            transformer.LinkTo(consumer, new DataflowLinkOptions { PropagateCompletion = true });

            await consumer.Completion
                .ContinueWith(
                    task =>
                    {
                        _logger.LogInformation("Finished processing device events");
                    },
                    cancellationToken,
                    AsyncContinueOnSuccess,
                    TaskScheduler.Current)
                .ConfigureAwait(false);
        }
    }
}