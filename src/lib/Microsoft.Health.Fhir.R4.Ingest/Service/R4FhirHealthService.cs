// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using EnsureThat;
using Hl7.Fhir.Rest;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class R4FhirHealthService :
        FhirHealthService
    {
        private readonly IFhirClient _client;

        public R4FhirHealthService(IFhirClient fhirClient)
        {
            _client = EnsureArg.IsNotNull(fhirClient, nameof(fhirClient));
        }

        public override Task<FhirHealthCheckStatus> CheckHealth()
        {
            try
            {
                Hl7.Fhir.Model.Bundle result = _client.Search<Hl7.Fhir.Model.StructureDefinition>();
                return Task.FromResult(new FhirHealthCheckStatus(string.Empty, 200));
            }
            catch (FhirOperationException ex)
            {
                return Task.FromResult(new FhirHealthCheckStatus(ex.Message, (int)ex.Status));
            }
            catch (IdentityModel.Clients.ActiveDirectory.AdalServiceException ex)
            {
                return Task.FromResult(new FhirHealthCheckStatus(ex.Message, ex.StatusCode));
            }
            catch (Exception ex)
            {
                return Task.FromResult(new FhirHealthCheckStatus(ex.Message, 500));
                throw;
            }
        }
    }
}
