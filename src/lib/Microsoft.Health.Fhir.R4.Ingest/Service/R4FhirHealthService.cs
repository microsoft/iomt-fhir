// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Hl7.Fhir.Rest;
using Microsoft.Health.Extensions.Fhir.Search;
using Microsoft.Health.Extensions.Fhir.Service;
using Microsoft.Health.Fhir.Client;
using Microsoft.Health.Fhir.Ingest.Data;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class R4FhirHealthService :
        FhirHealthService
    {
        private readonly IFhirService _fhirService;

        public R4FhirHealthService(IFhirService fhirService)
        {
            _fhirService = EnsureArg.IsNotNull(fhirService, nameof(fhirService));
        }

        public override async Task<FhirHealthCheckStatus> CheckHealth(CancellationToken token = default)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    SearchParams search = new SearchParams().SetCount(1);
                    Hl7.Fhir.Model.Bundle result = await _fhirService.SearchForResourceAsync(Hl7.Fhir.Model.ResourceType.StructureDefinition, query: null, search.Count, token).ConfigureAwait(false);
                    return await Task.FromResult(new FhirHealthCheckStatus(string.Empty, 200));
                }

                token.ThrowIfCancellationRequested();
                return await Task.FromResult(new FhirHealthCheckStatus(token.ToString(), 500));
            }
            catch (FhirException ex)
            {
                return await Task.FromResult(new FhirHealthCheckStatus(ex.Message, (int)ex.StatusCode));
            }
            catch (IdentityModel.Clients.ActiveDirectory.AdalServiceException ex)
            {
                return await Task.FromResult(new FhirHealthCheckStatus(ex.Message, ex.StatusCode));
            }
#pragma warning disable CA1031
            catch (Exception ex)
#pragma warning restore CA1031
            {
                return await Task.FromResult(new FhirHealthCheckStatus(ex.Message, 500));
            }
        }
    }
}
