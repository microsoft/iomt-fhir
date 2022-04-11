// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
        private readonly int _asyncCollectorBatchSize;
        private readonly IExceptionTelemetryProcessor _exceptionTelemetryProcessor;

        public MeasurementEventNormalizationService(
            ITelemetryLogger log,
            IContentTemplate contentTemplate,
            IExceptionTelemetryProcessor exceptionTelemetryProcessor)
            : this(log, contentTemplate, new EventDataWithJsonBodyToJTokenConverter(), exceptionTelemetryProcessor, 1)
        {
        }

        public MeasurementEventNormalizationService(
            ITelemetryLogger log,
            IContentTemplate contentTemplate,
            Data.IConverter<EventData, JToken> converter,
            IExceptionTelemetryProcessor exceptionTelemetryProcessor,
            int maxParallelism,
            int asyncCollectorBatchSize = 200)
        {
            _log = EnsureArg.IsNotNull(log, nameof(log));
            _contentTemplate = EnsureArg.IsNotNull(contentTemplate, nameof(contentTemplate));
            _converter = EnsureArg.IsNotNull(converter, nameof(converter));
            _exceptionTelemetryProcessor = EnsureArg.IsNotNull(exceptionTelemetryProcessor, nameof(exceptionTelemetryProcessor));
            _maxParallelism = maxParallelism;
            _asyncCollectorBatchSize = EnsureArg.IsGt(asyncCollectorBatchSize, 0, nameof(asyncCollectorBatchSize));
        }

        public async Task ProcessAsync(IEnumerable<EventData> data, IAsyncCollector<IMeasurement> collector, Func<Exception, EventData, Task<bool>> errorConsumer = null)
        {
            EnsureArg.IsNotNull(data, nameof(data));
            EnsureArg.IsNotNull(collector, nameof(collector));

            await StartConsumer(StartProducer(data), new EnumerableAsyncCollectorFacade<IMeasurement>(collector), errorConsumer ?? ProcessErrorAsync).ConfigureAwait(false);
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
            var transformingConsumer = new TransformManyBlock<EventData, (string, IMeasurement)>(
                async evt =>
                {
                    var createdMeasurements = new List<(string, IMeasurement)>();
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
                            createdMeasurements.Add((partitionId, measurement));
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

            var asyncCollectorConsumer = new ActionBlock<(string, IMeasurement)[]>(
                async partitionIdAndeasurements =>
                {
                    try
                    {
                        var measurements = partitionIdAndeasurements.Select(pm => pm.Item2);
                        await collector.AddAsync(measurements, cts.Token).ConfigureAwait(false);

                        foreach (var partitionAndMeasurment in partitionIdAndeasurements)
                        {
                            _log.LogMetric(IomtMetrics.NormalizedEvent(partitionAndMeasurment.Item1), 1);
                        }
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
            var batchBlock = new BatchBlock<(string, IMeasurement)>(_asyncCollectorBatchSize);
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

        /// <summary>
        /// Default error processor that returns true if the exception is handled and false otherwise.
        /// </summary>
        /// <param name="ex">The exception to be processed.</param>
        /// <param name="data">Event data that encountered an error upon processing.</param>
        /// <returns>Returns true if the exception is handled and false otherwise.</returns>
        private Task<bool> ProcessErrorAsync(Exception ex, EventData data)
        {
            var handled = _exceptionTelemetryProcessor.HandleException(ex, _log);
            return Task.FromResult(!handled);
        }
    }
}