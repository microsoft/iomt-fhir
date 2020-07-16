// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Common.Handler
{
    /// <summary>
    /// Define a handler in a chain of responsibility
    /// </summary>
    /// <typeparam name="TRequest">The type of request the handler will evaluate.</typeparam>
    /// <typeparam name="TResult">The type of result the handler will generate.</typeparam>
    public interface IResponsibilityHandler<in TRequest, out TResult>
        where TResult : class
    {
        /// <summary>
        /// Evalue the request with chained responsibility handlers.
        /// </summary>
        /// <param name="request">The request to be evaluated.</param>
        /// <returns>The evaluation result.</returns>
        TResult Evaluate(TRequest request);
    }

    /// <summary>
    /// Define a handler in a chain of responsibility that can share a context container.
    /// </summary>
    /// <typeparam name="TRequest">The type of request the handler will evaluate.</typeparam>
    /// <typeparam name="TResult">The type of result the handler will generate.</typeparam>
    /// <typeparam name="TContext">The type of context container</typeparam>
    public interface IResponsibilityHandler<in TRequest, out TResult, TContext>
        where TResult : class
    {
        /// <summary>
        /// Evalue the request with chained responsibility handlers.
        /// </summary>
        /// <param name="request">The request to be evaluated.</param>
        /// <returns>The evaluation result.</returns>
        TResult Evaluate(TRequest request);

        /// <summary>
        /// Evalue the request with chained responsibility handlers with a sharable
        /// context container.
        /// </summary>
        /// <typeparam name="TContext">The type of a context container shared by chained handlers.</typeparam>
        /// <param name="request">The request to be evaluated.</param>
        /// <param name="context">The context container.</param>
        /// <returns>The evaluation result.</returns>
        TResult Evaluate(TRequest request, out TContext context);
    }
}
