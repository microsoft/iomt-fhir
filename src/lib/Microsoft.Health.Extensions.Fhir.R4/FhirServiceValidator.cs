// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Health.Extensions.Fhir.Telemetry.Exceptions;
using Microsoft.Health.Logging.Telemetry;
using IFhirClient = Microsoft.Health.Fhir.Client.IFhirClient;

namespace Microsoft.Health.Extensions.Fhir
{
    public static class FhirServiceValidator
    {
        public static async Task<bool> ValidateFhirServiceAsync(
            IFhirClient client,
            string url,
            ITelemetryLogger logger)
        {
            EnsureArg.IsNotNull(client, nameof(client));
            EnsureArg.IsNotNullOrWhiteSpace(url, nameof(url));
            EnsureArg.IsNotNull(logger, nameof(logger));

            try
            {
                await client.ReadAsync<Hl7.Fhir.Model.CapabilityStatement>(url).ConfigureAwait(false);
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
