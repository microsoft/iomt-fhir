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
        TResult Evaluate(TRequest request);
    }
}
