﻿// -------------------------------------------------------------------------------------------------
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
        private readonly IConverter<EventData, JToken> _converter = null;
        private readonly int _maxParallelism;
        private readonly ITelemetryLogger _log;
        private readonly int _asyncCollectorBatchSize;

        public MeasurementEventNormalizationService(ITelemetryLogger log, IContentTemplate contentTemplate)
            : this(log, contentTemplate, new EventDataWithJsonBodyToJTokenConverter(), 3)
        {
        }

        public MeasurementEventNormalizationService(ITelemetryLogger log, IContentTemplate contentTemplate, IConverter<EventData, JToken> converter, int maxParallelism, int asyncCollectorBatchSize = 200)
        {
            _log = EnsureArg.IsNotNull(log, nameof(log));
            _contentTemplate = EnsureArg.IsNotNull(contentTemplate, nameof(contentTemplate));
            _converter = EnsureArg.IsNotNull(converter, nameof(converter));
            _maxParallelism = maxParallelism;
            _asyncCollectorBatchSize = EnsureArg.IsGt(asyncCollectorBatchSize, 0, nameof(asyncCollectorBatchSize));
        }

        public async Task ProcessAsync(IEnumerable<EventData> data, IEnumerableAsyncCollector<IMeasurement> collector, Func<Exception, EventData, Task<bool>> errorConsumer = null)
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

        private async Task StartConsumer(ISourceBlock<EventData> producer, IEnumerableAsyncCollector<IMeasurement> collector, Func<Exception, EventData, Task<bool>> errorConsumer)
        {
            // Collect non operation canceled exceptions as they occur to ensure the entire data stream is processed
            var exceptions = new ConcurrentBag<Exception>();
            var cts = new CancellationTokenSource();
            var transformingConsumer = new TransformManyBlock<EventData, IMeasurement>(
                async evt =>
                {
                    var createdMeasurements = new List<IMeasurement>();
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
                            createdMeasurements.Add(measurement);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (await errorConsumer(ex, evt).ConfigureAwait(false))
                        {
                            exceptions.Add(ex);
                        }
                    }

                    return createdMeasurements;
                });

            var asyncCollectorConsumer = new ActionBlock<IMeasurement[]>(
                async measurements =>
                {
                    try
                    {
                        await collector.AddAsync(measurements, cts.Token).ConfigureAwait(false);
                        _log.LogMetric(
                            IomtMetrics.NormalizedEvent(partitionId),
                            measurements.Length);
                    }
                    catch (OperationCanceledException)
                    {
                        cts.Cancel();
                        throw;
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                },
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxParallelism, SingleProducerConstrained = true, CancellationToken = cts.Token });

            // Connect the input EventData to the transformer block
            producer.LinkTo(transformingConsumer, new DataflowLinkOptions { PropagateCompletion = true });

            // Batch the produced IMeasurements
            var batchBlock = new BatchBlock<IMeasurement>(_asyncCollectorBatchSize);
            transformingConsumer.LinkTo(batchBlock, new DataflowLinkOptions { PropagateCompletion = true });

            // Connect the final action of writing events into EventHub
            batchBlock.LinkTo(asyncCollectorConsumer, new DataflowLinkOptions { PropagateCompletion = true });

            await asyncCollectorConsumer.Completion
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