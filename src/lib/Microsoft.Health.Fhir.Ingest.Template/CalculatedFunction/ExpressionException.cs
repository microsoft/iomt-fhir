// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Health.Fhir.Ingest.Template.CalculatedFunction
{
    public class ExpressionException : Exception
    {
        public ExpressionException(string message)
            : base(message)
        {
        }

        public ExpressionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public ExpressionException()
        {
        }
    }
}
