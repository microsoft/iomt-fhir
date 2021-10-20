// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using EnsureThat;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Telemetry;
using Microsoft.Health.Fhir.Ingest.Template;
using Microsoft.Health.Logging.Telemetry;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class MeasurementEventNormalizationService : IDataNormalizationService<EventData, IMeasurement>
    {
        private const TaskContinuationOptions AsyncContinueOnSuccess = TaskContinuationOptions.RunContinuationsAsynchronously | TaskContinuationOptions.NotOnFaulted | TaskContinuationOptions.NotOnCanceled;
        private readonly IContentTemplate _contentTemplate = null;
        private readonly Data.IConverter<EventData, JToken> _converter = null;
        private readonly int _maxParallelism;
        private readonly ITelemetryLogger _log;

        public MeasurementEventNormalizationService(ITelemetryLogger log, IContentTemplate contentTemplate)
            : this(log, contentTemplate, new EventDataWithJsonBodyToJTokenConverter(), 3)
        {
        }

        public MeasurementEventNormalizationService(ITelemetryLogger log, IContentTemplate contentTemplate, Data.IConverter<EventData, JToken> converter, int maxParallelism)
        {
            _log = EnsureArg.IsNotNull(log, nameof(log));
            _contentTemplate = EnsureArg.IsNotNull(contentTemplate, nameof(contentTemplate));
            _converter = EnsureArg.IsNotNull(converter, nameof(converter));
            _maxParallelism = maxParallelism;
        }

        public async Task ProcessAsync(IEnumerable<EventData> data, IAsyncCollector<IMeasurement> collector, Func<Exception, EventData, Task<bool>> errorConsumer = null)
        {
            EnsureArg.IsNotNull(data, nameof(data));
            EnsureArg.IsNotNull(collector, nameof(collector));

            await StartConsumer(StartProducer(data), collector, errorConsumer ?? ProcessErrorAsync).ConfigureAwait(false);
        }

        private static ISourceBlock<EventData> StartProducer(IEnumerable<EventData> data)
        {
            var producer = new BufferBlock<EventData>(new DataflowBlockOptions { BoundedCapacity = DataflowBlockOptions.Unbounded });

            _ = Task.Run(async () =>
              {
                  foreach (var evt in data)
                  {
                      while (!await producer.SendAsync(evt))
                      {
                          await Task.Yield();
                      }
                  }

                  producer.Complete();
              });

            return producer;
        }

        private async Task StartConsumer(ISourceBlock<EventData> producer, IAsyncCollector<IMeasurement> collector, Func<Exception, EventData, Task<bool>> errorConsumer)
        {
            // Collect non operation canceled exceptions as they occur to ensure the entire data stream is processed
            var exceptions = new ConcurrentBag<Exception>();
            var cts = new CancellationTokenSource();
            var consumer = new ActionBlock<EventData>(
                async evt =>
                {
                    try
                    {
                        string partitionId = evt.SystemProperties.PartitionKey;
                        var deviceEventProcessingLatency = DateTime.UtcNow - evt.SystemProperties.EnqueuedTimeUtc;

                        _log.LogMetric(
                            IomtMetrics.DeviceEventProcessingLatency(partitionId),
                            deviceEventProcessingLatency.TotalSeconds);

                        _log.LogMetric(
                            IomtMetrics.DeviceEventProcessingLatencyMs(partitionId),
                            deviceEventProcessingLatency.TotalMilliseconds);

                        var token = _converter.Convert(evt);

                        foreach (var measurement in _contentTemplate.GetMeasurements(token))
                        {
                            measurement.IngestionTimeUtc = evt.SystemProperties.EnqueuedTimeUtc;
                            await collector.AddAsync(measurement).ConfigureAwait(false);

                            _log.LogMetric(
                                IomtMetrics.NormalizedEvent(partitionId),
                                1);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        cts.Cancel();
                        throw;
                    }
#pragma warning disable CA1031
                    catch (Exception ex)
                    {
                        if (await errorConsumer(ex, evt).ConfigureAwait(false))
                        {
                            exceptions.Add(ex);
                        }
                    }
#pragma warning restore CA1031
                },
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxParallelism, SingleProducerConstrained = true, CancellationToken = cts.Token });

            _ = producer.LinkTo(consumer, new DataflowLinkOptions { PropagateCompletion = true });

            await consumer.Completion
                .ContinueWith(
                    task =>
                    {
                        if (!exceptions.IsEmpty)
                        {
                            throw new AggregateException(exceptions);
                        }
                    },
                    cts.Token,
                    AsyncContinueOnSuccess,
                    TaskScheduler.Current)
                .ConfigureAwait(false);
        }

        private async Task<bool> ProcessErrorAsync(Exception ex, EventData data)
        {
            // Default error processor
            return await Task.FromResult(true).ConfigureAwait(false);
        }
    }
}