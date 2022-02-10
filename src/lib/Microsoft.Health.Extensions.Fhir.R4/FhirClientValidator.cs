// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Health.Extensions.Fhir.Telemetry.Exceptions;
using Microsoft.Health.Fhir.Client;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Extensions.Fhir
{
    public static class FhirClientValidator
    {
        public static async Task<bool> ValidateFhirClientAsync(
            this HttpClient client,
            ITelemetryLogger logger)
        {
            EnsureArg.IsNotNull(client, nameof(client));
            EnsureArg.IsNotNull(logger, nameof(logger));

            try
            {
                var fhirClient = new FhirClient(client);
                await fhirClient.ReadAsync<Hl7.Fhir.Model.CapabilityStatement>("metadata").ConfigureAwait(false);
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
