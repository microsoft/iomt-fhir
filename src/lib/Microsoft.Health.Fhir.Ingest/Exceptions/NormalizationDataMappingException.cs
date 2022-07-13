// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Text;
using EnsureThat;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class NormalizationDataMappingException : Exception
    {
        public NormalizationDataMappingException(Exception ex)
            : base(BuildMessage(ex), ex)
        {
            EnsureArg.IsNotNull(ex, nameof(ex));
        }

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
