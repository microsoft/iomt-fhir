// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Net.Http.Headers;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Specification;

namespace Microsoft.Health.Extensions.Fhir.Client
{
    public class FhirClient : BaseFhirClient
    {
        public FhirClient(
            Uri endpoint,
            IClientRequester httpClientRequester,
            FhirClientSettings settings = null,
            IStructureDefinitionSummaryProvider provider = null)
            : base(endpoint, settings, provider)
        {
            Requester = httpClientRequester;

            // Expose default request headers to user.
            if (httpClientRequester is HttpClientRequester requesterCustom)
            {
                RequestHeaders = requesterCustom.Client.DefaultRequestHeaders;
            }
        }

        /// <summary>
        /// Default request headers that can be modified to persist default headers to internal client.
        /// </summary>
        public HttpRequestHeaders RequestHeaders { get; protected set; }

        /// <summary>
        /// Override dispose in order to clean up request headers tied to disposed requester.
        /// </summary>
        /// <param name="disposing">if we are disposing</param>
        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    RequestHeaders = null;
                    base.Dispose(disposing);
                }

                disposedValue = true;
            }
        }
    }
}
