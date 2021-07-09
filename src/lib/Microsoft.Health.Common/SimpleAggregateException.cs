// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using EnsureThat;

namespace Microsoft.Health.Common
{
    public class SimpleAggregateException : Exception
    {
        private readonly string _aggregatedMessage;

        public SimpleAggregateException(IEnumerable<Exception> exceptions)
            : this(new List<Exception>(exceptions))
        {
        }

        public SimpleAggregateException(ICollection<Exception> exceptions)
            : this("One or more exceptions have occured", exceptions)
        {
        }

        public SimpleAggregateException(string message, ICollection<Exception> exceptions)
        {
            EnsureArg.IsNotNullOrWhiteSpace(message, nameof(message));
            EnsureArg.HasItems(exceptions, nameof(exceptions));

            _aggregatedMessage = ConstructMessage(message, exceptions);
            InnerExceptions = new List<Exception>(exceptions);
        }

        public override string Message => _aggregatedMessage;

        public IReadOnlyCollection<Exception> InnerExceptions { get; }

        private string ConstructMessage(string message, IEnumerable<Exception> exceptions)
        {
            var exceptionMessage = new StringBuilder(message);

            exceptionMessage.AppendLine(":");

            foreach (var exception in exceptions)
            {
                exceptionMessage.AppendLine($"---- {exception.GetType()}: {exception.Message}");
            }

            return exceptionMessage.ToString();
        }
    }
}
