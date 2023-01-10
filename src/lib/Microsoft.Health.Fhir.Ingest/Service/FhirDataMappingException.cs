﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Common.Telemetry.Exceptions;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class FhirDataMappingException : ThirdPartyLoggedFormattableException
    {
        private static readonly string _errorType = ErrorType.FHIRConversionError;

        public FhirDataMappingException(
            string message,
            Exception innerException,
            string errorName)
            : base(
                  message,
                  innerException,
                  name: $"{_errorType}{errorName}",
                  operation: ConnectorOperation.FHIRConversion)
        {
        }

        public override string ErrType => _errorType;

        public override string ErrSeverity => ErrorSeverity.Critical;

        public override string ErrSource => nameof(ErrorSource.User);
    }
}