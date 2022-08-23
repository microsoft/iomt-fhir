// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Common.Telemetry.Exceptions;

namespace Microsoft.Health.Extensions.Fhir
{
    public class MultipleResourceFoundException<T> : ThirdPartyLoggedFormattableException
    {
        public MultipleResourceFoundException(int resourceCount)
            : base(GenerateErrorMessage<T>(resourceCount, null))
        {
        }

        public MultipleResourceFoundException()
        {
        }

        public MultipleResourceFoundException(int resourceCount, IEnumerable<string> ids)
            : base(GenerateErrorMessage<T>(resourceCount, ids))
        {
        }

        public MultipleResourceFoundException(string message)
            : base(message)
        {
        }

        public MultipleResourceFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public override string ErrName => $"Multiple{typeof(T).Name}FoundException";

        public override string ErrType => ErrorType.FHIRResourceError;

        public override string Operation => ConnectorOperation.FHIRConversion;

        private static string GenerateErrorMessage<TResource>(int resourceCount, IEnumerable<string> ids)
        {
            var sb = new StringBuilder($"Multiple resources {resourceCount} of type {typeof(T)} found, expected one.");

            if (ids != null)
            {
                sb.Append(" Resource internal ids: ");
                sb.Append(string.Join(", ", ids));
                sb.Append(".");
            }

            return sb.ToString();
        }
    }
}
