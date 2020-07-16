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

        public static IResponsibilityHandler<TRequest, TResult, TContext> Chain<TRequest, TResult, TContext>(this IResponsibilityHandler<TRequest, TResult, TContext> link, IResponsibilityHandler<TRequest, TResult, TContext> successor)
            where TResult : class
        {
            return new NullNextResponsibilityHandler<TRequest, TResult, TContext>(link, successor);
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

        private class NullNextResponsibilityHandler<TRequest, TResult, TContext> : IResponsibilityHandler<TRequest, TResult, TContext>
                where TResult : class
        {
            private readonly IResponsibilityHandler<TRequest, TResult, TContext> _predecessor;
            private readonly IResponsibilityHandler<TRequest, TResult, TContext> _succesor;

            public NullNextResponsibilityHandler(IResponsibilityHandler<TRequest, TResult, TContext> predecessor, IResponsibilityHandler<TRequest, TResult, TContext> succesor)
            {
                _predecessor = predecessor;
                _succesor = succesor;
            }

            public TResult Evaluate(TRequest request)
            {
                return _predecessor.Evaluate(request) ?? _succesor.Evaluate(request);
            }

            public TResult Evaluate(TRequest request, out TContext context)
            {
                return _predecessor.Evaluate(request, out context) ?? _succesor.Evaluate(request, out context);
            }
        }
    }
}
