// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;
using Hl7.Fhir.Rest;
using Microsoft.Health.Extensions.Fhir.Telemetry.Exceptions;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Extensions.Fhir
{
    public static class FhirServiceValidator
    {
        public static void ValidateFhirService(FhirClient client, ITelemetryLogger logger)
        {
            EnsureArg.IsNotNull(client, nameof(client));

            try
            {
                client.CapabilityStatement(SummaryType.True);
            }
            catch (Exception exception)
            {
                FhirServiceExceptionProcessor.ProcessException(exception, logger);
            }
        }
    }
}
