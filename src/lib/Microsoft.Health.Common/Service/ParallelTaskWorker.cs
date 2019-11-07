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

namespace Microsoft.Health.Common.Service
{
    public abstract class ParallelTaskWorker<TOptions>
        where TOptions : class
    {
        private const TaskContinuationOptions AsyncContinueOnSuccess = TaskContinuationOptions.RunContinuationsAsynchronously | TaskContinuationOptions.NotOnFaulted | TaskContinuationOptions.NotOnCanceled;
        private readonly TOptions _options;
        private readonly int _maxParallelism;

        protected ParallelTaskWorker(TOptions options, int maxParallelism = 1)
        {
            _options = EnsureArg.IsNotNull(options, nameof(options));
            _maxParallelism = EnsureArg.IsGt(maxParallelism, 0, nameof(maxParallelism));
        }

        protected TOptions Options => _options;

        protected virtual async Task StartWorker(IEnumerable<Func<Task>> workItems)
        {
            // Collect non operation canceled exceptions as they occur to ensure the entire data stream is processed
            var exceptions = new ConcurrentBag<Exception>();
            var cts = new CancellationTokenSource();
            var consumer = new ActionBlock<Func<Task>>(
                async work =>
                {
                    try
                    {
                        await work().ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        cts.Cancel();
                        throw;
                    }
#pragma warning disable CA1031
                    catch (Exception ex)
                    {
                        if (await ErrorHandlerAsync(ex).ConfigureAwait(false))
                        {
                            exceptions.Add(ex);
                        }
                    }
#pragma warning restore CA1031
                },
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxParallelism, SingleProducerConstrained = true, CancellationToken = cts.Token });

            _ = StartProducer(workItems)
                .LinkTo(consumer, new DataflowLinkOptions { PropagateCompletion = true });

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

        /// <summary>
        /// Additional logic for handling exceptions.
        /// </summary>
        /// <param name="ex">Exception to evaluate.</param>
        /// <returns>True if the exception should be included in the aggregate exception. Returning false will cause the exception to be ignored.</returns>
        protected virtual Task<bool> ErrorHandlerAsync(Exception ex)
        {
            return Task.FromResult(true);
        }

        private static ISourceBlock<Func<Task>> StartProducer(IEnumerable<Func<Task>> workItems)
        {
            var producer = new BufferBlock<Func<Task>>(new DataflowBlockOptions { BoundedCapacity = DataflowBlockOptions.Unbounded });

            _ = Task.Run(async () =>
            {
                foreach (var item in workItems)
                {
                    while (!await producer.SendAsync(item))
                    {
                        await Task.Yield();
                    }
                }

                producer.Complete();
            });

            return producer;
        }
    }
}
