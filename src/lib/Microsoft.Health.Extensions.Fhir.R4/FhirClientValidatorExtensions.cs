// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Health.Extensions.Fhir.Telemetry.Exceptions;
using Microsoft.Health.Fhir.Client;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Extensions.Fhir
{
    public static class FhirClientValidatorExtensions
    {
        public static async Task<bool> ValidateFhirClientAsync(
            this IFhirClient client,
            ITelemetryLogger logger)
        {
            EnsureArg.IsNotNull(client, nameof(client));
            EnsureArg.IsNotNull(logger, nameof(logger));

            try
            {
                await client.ReadAsync<Hl7.Fhir.Model.CapabilityStatement>("metadata?_summary=true").ConfigureAwait(false);
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
