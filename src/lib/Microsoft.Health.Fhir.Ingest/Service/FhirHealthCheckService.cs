// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Health.Fhir.Ingest.Data;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class FhirHealthCheckService
    {
        private readonly FhirHealthService _fhirHealthService;

        public FhirHealthCheckService(FhirHealthService fhirHealthService)
        {
            _fhirHealthService = EnsureArg.IsNotNull(fhirHealthService, nameof(fhirHealthService));
        }

        public async Task<FhirHealthCheckStatus> CheckHealth(CancellationToken token = default)
        {
            return await _fhirHealthService.CheckHealth(token);
        }
    }
}
