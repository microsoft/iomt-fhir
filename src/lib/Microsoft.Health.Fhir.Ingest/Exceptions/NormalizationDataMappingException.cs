// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Text;
using EnsureThat;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Common.Telemetry.Exceptions;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class NormalizationDataMappingException : ThirdPartyLoggedFormattableException
    {
        private static readonly string _errorType = ErrorType.DeviceTemplateError;

        public NormalizationDataMappingException(
            Exception innerException,
            string errorName)
            : base(
                  BuildMessage(innerException),
                  innerException,
                  name: $"{_errorType}{errorName}",
                  operation: ConnectorOperation.Normalization)
        {
            EnsureArg.IsNotNull(innerException, nameof(innerException));
        }

        public override string ErrType => _errorType;

        public override string ErrSeverity => ErrorSeverity.Critical;

        public override string ErrSource => nameof(ErrorSource.User);

        private static string BuildMessage(Exception ex)
        {
            EnsureArg.IsNotNull(ex, nameof(ex));

            StringBuilder sb = new (ex.Message);

            Exception innerException = ex.InnerException;
            int exceptionCount = 0;
            while (innerException != null)
            {
                sb.Append($"\n{++exceptionCount}:{innerException.Message}");
                innerException = innerException.InnerException;
            }

            return sb.ToString();
        }
    }
}
