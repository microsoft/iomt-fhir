// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Common.Handler
{
    public static class ResponsibilityHandlerExtensions
    {
        public static IResponsibilityHandler<TRequest, TResult> Chain<TRequest, TResult>(this IResponsibilityHandler<TRequest, TResult> link, IResponsibilityHandler<TRequest, TResult> successor)
            where TResult : class
        {
            return new NullNextResponsibilityHandler<TRequest, TResult>(link, successor);
        }

        private class NullNextResponsibilityHandler<TRequest, TResult> : IResponsibilityHandler<TRequest, TResult>
                where TResult : class
        {
            private readonly IResponsibilityHandler<TRequest, TResult> _predecessor;
            private readonly IResponsibilityHandler<TRequest, TResult> _succesor;

            public NullNextResponsibilityHandler(IResponsibilityHandler<TRequest, TResult> predecessor, IResponsibilityHandler<TRequest, TResult> succesor)
            {
                _predecessor = predecessor;
                _succesor = succesor;
            }

            public TResult Evaluate(TRequest request)
            {
                return _predecessor.Evaluate(request) ?? _succesor.Evaluate(request);
            }
        }
    }
}
