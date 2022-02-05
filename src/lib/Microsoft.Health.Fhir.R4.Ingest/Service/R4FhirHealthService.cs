// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Hl7.Fhir.Rest;
using Microsoft.Health.Extensions.Fhir.Repository;
using Microsoft.Health.Extensions.Fhir.Search;
using Microsoft.Health.Fhir.Ingest.Data;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class R4FhirHealthService :
        FhirHealthService
    {
        private readonly IFhirServiceRepository _client;

        public R4FhirHealthService(IFhirServiceRepository fhirClient)
        {
            _client = EnsureArg.IsNotNull(fhirClient, nameof(fhirClient));
        }

        public override async Task<FhirHealthCheckStatus> CheckHealth(CancellationToken token = default)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    SearchParams search = new SearchParams().SetCount(1);
                    await _client.SearchForResourceAsync(Hl7.Fhir.Model.ResourceType.Bundle, query: null, search.Count, token).ConfigureAwait(false);
                    return await Task.FromResult(new FhirHealthCheckStatus(string.Empty, 200));
                }

                token.ThrowIfCancellationRequested();
                return await Task.FromResult(new FhirHealthCheckStatus(token.ToString(), 500));
            }
            catch (FhirOperationException ex)
            {
                return await Task.FromResult(new FhirHealthCheckStatus(ex.Message, (int)ex.Status));
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
