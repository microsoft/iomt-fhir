// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Health.Fhir.Ingest.Data
{
    public class CorrelationIdNotDefinedException : Exception
    {
        public CorrelationIdNotDefinedException(string message)
            : base(message)
        {
        }

        public CorrelationIdNotDefinedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public CorrelationIdNotDefinedException()
        {
        }
    }
}
