// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Health.Extensions.Fhir.Service;
using Microsoft.Health.Extensions.Fhir.Telemetry.Exceptions;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Extensions.Fhir
{
    public static class FhirServiceValidator
    {
        public static async Task<bool> ValidateFhirServiceAsync(
            IFhirService fhirService,
            ITelemetryLogger logger)
        {
            EnsureArg.IsNotNull(fhirService, nameof(fhirService));
            EnsureArg.IsNotNull(logger, nameof(logger));

            try
            {
                await fhirService.ReadResourceAsync<Hl7.Fhir.Model.CapabilityStatement>("metadata").ConfigureAwait(false);
                return true;
            }
            catch (Exception exception)
            {
                FhirServiceExceptionProcessor.ProcessException(exception, logger);
                return false;
            }
        }
    }
}
