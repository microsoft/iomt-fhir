// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using EnsureThat;
using Hl7.Fhir.Rest;
using Microsoft.Extensions.Options;
using Microsoft.Health.Common;

namespace Microsoft.Health.Extensions.Fhir
{
#pragma warning disable CA1063 // Implement IDisposable Correctly
    public class FhirClientFactory : IFactory<BaseFhirClient>, IDisposable
#pragma warning restore CA1063 // Implement IDisposable Correctly
    {
        private readonly FhirClientFactoryOptions _options;
        private HttpClient _httpClient;
        private readonly SemaphoreSlim _semaphoreSlim;

        public FhirClientFactory(IOptions<FhirClientFactoryOptions> options)
        {
            _options = EnsureArg.IsNotNull(options?.Value, nameof(options));
            _semaphoreSlim = new SemaphoreSlim(1, 1);
        }

        public BaseFhirClient Create()
        {
            EnsureArg.IsNotNull(_options.CredentialProvider);
            var tokenCredential = _options.CredentialProvider.GetCredential();
            return CreateClient(tokenCredential);
        }

        private BaseFhirClient CreateClient(TokenCredential tokenCredential)
        {
            var url = Environment.GetEnvironmentVariable("FhirService:Url");
            EnsureArg.IsNotNullOrEmpty(url, nameof(url));

            EnsureArg.IsNotNull(tokenCredential, nameof(tokenCredential));

            var fhirClientSettings = new FhirClientSettings
            {
                PreferredFormat = ResourceFormat.Json,
            };

            var uri = new Uri(url);
            var httpClient = GetHttpClient(() =>
                new BearerTokenAuthorizationMessageHandler(url, tokenCredential)
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                });
            var requester = new Client.HttpClientRequester(uri, fhirClientSettings, httpClient);

#pragma warning disable CA2000 // Dispose objects before losing scope
            var client = new Client.FhirClient(uri, requester, fhirClientSettings);
#pragma warning restore CA2000 // Dispose objects before losing scope

            return client;
        }

        private HttpClient GetHttpClient(Func<HttpClientHandler> handlerGenerator)
        {
            if (_httpClient == null)
            {
                try
                {
                    _semaphoreSlim.Wait();
                    if (_httpClient == null)
                    {
                        _httpClient = new HttpClient(handlerGenerator.Invoke());
                        _httpClient.DefaultRequestHeaders.Add("User-Agent", $".NET FhirClient for FHIR");
                        _httpClient.Timeout = TimeSpan.FromSeconds(30);
                    }
                }
                finally
                {
                    _semaphoreSlim.Release();
                }
            }

            return _httpClient;
        }

#pragma warning disable CA1063 // Implement IDisposable Correctly
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
        public void Dispose()
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
#pragma warning restore CA1063 // Implement IDisposable Correctly
        {
            _httpClient.Dispose();
        }

        private class BearerTokenAuthorizationMessageHandler : HttpClientHandler
        {
            public BearerTokenAuthorizationMessageHandler(string url, TokenCredential tokenCredential)
            {
                TokenCredential = EnsureArg.IsNotNull(tokenCredential, nameof(tokenCredential));
                Uri = new Uri(url);
                Scopes = new string[] { $"{Uri}.default" };
            }

            private TokenCredential TokenCredential { get; }

            private Uri Uri { get; }

            private string[] Scopes { get; }

            private AccessToken AccessToken { get; set; }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var requestContext = new TokenRequestContext(Scopes);

                if (string.IsNullOrEmpty(AccessToken.Token) || (AccessToken.ExpiresOn < DateTime.UtcNow.AddMinutes(1)))
                {
                    AccessToken = await TokenCredential.GetTokenAsync(requestContext, CancellationToken.None);
                }

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken.Token);
                var response = await base.SendAsync(request, cancellationToken);
                return response;
            }
        }
    }
}
