// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Utility;

namespace Microsoft.Health.Extensions.Fhir.Client
{
    public class HttpClientRequester : IClientRequester, IDisposable
    {
        private bool disposedValue = false; // To detect redundant calls

        public HttpClientRequester(Uri baseUrl, FhirClientSettings settings, HttpClient httpClient)
        {
            Settings = settings;
            BaseUrl = baseUrl;
            Client = httpClient;
        }

        public FhirClientSettings Settings { get; set; }

        public Uri BaseUrl { get; private set; }

        public HttpClient Client { get; private set; }

        public EntryResponse LastResult { get; private set; }

        public EntryResponse Execute(EntryRequest interaction)
        {
            return ExecuteAsync(interaction).WaitResult();
        }

        public async Task<EntryResponse> ExecuteAsync(EntryRequest interaction)
        {
            if (interaction == null)
            {
                throw Error.ArgumentNull(nameof(interaction));
            }

            using var requestMessage = interaction.ToHttpRequestMessage(BaseUrl, Settings);
            if (Settings.PreferCompressedResponses)
            {
                requestMessage.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                requestMessage.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
            }

            byte[] outgoingBody = null;
            if (requestMessage.Method == HttpMethod.Post || requestMessage.Method == HttpMethod.Put)
            {
                outgoingBody = await requestMessage.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            }

            using var response = await Client.SendAsync(requestMessage).ConfigureAwait(false);
            try
            {
                var body = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

                LastResult = response.ToEntryResponse(body);
                return LastResult;
            }
            catch (AggregateException ae)
            {
                throw ae.GetBaseException();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                disposedValue = true;
            }
        }
    }
}
