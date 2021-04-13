// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using EnsureThat;
using Hl7.Fhir.Rest;
using Microsoft.Extensions.Options;
using Microsoft.Health.Common;
using Microsoft.Health.Common.Auth;
using Microsoft.Health.Extensions.Fhir.Config;
using Microsoft.Health.Extensions.Host.Auth;

namespace Microsoft.Health.Extensions.Fhir
{
    public class FhirClientFactory : IFactory<FhirClient>
    {
        private readonly bool _useManagedIdentity = false;
        private readonly IAzureCredentialProvider _tokenCredentialProvider;

        public FhirClientFactory(IOptions<FhirClientFactoryOptions> options)
            : this(EnsureArg.IsNotNull(options, nameof(options)).Value.UseManagedIdentity)
        {
        }

        private FhirClientFactory()
            : this(useManagedIdentity: false)
        {
        }

        private FhirClientFactory(bool useManagedIdentity)
        {
            _useManagedIdentity = useManagedIdentity;
        }

        public FhirClientFactory(IAzureCredentialProvider provider)
        {
            _tokenCredentialProvider = provider;
        }

        public static IFactory<FhirClient> Instance { get; } = new FhirClientFactory();

        public FhirClient Create()
        {
            if (_tokenCredentialProvider != null)
            {
                return CreateClient(_tokenCredentialProvider.GetCredential());
            }

            return _useManagedIdentity ? CreateManagedIdentityClient() : CreateConfidentialApplicationClient();
        }

        private static FhirClient CreateClient(TokenCredential tokenCredential)
        {
            var url = Environment.GetEnvironmentVariable("FhirService:Url");
            EnsureArg.IsNotNullOrEmpty(url, nameof(url));
            var uri = new Uri(url);

            EnsureArg.IsNotNull(tokenCredential, nameof(tokenCredential));

            var fhirClientSettings = new FhirClientSettings
            {
                PreferredFormat = ResourceFormat.Json,
            };

            var client = new FhirClient(url, fhirClientSettings, new BearerTokenAuthorizationMessageHandler(uri, tokenCredential));

            return client;
        }

        private static FhirClient CreateManagedIdentityClient()
        {
            return CreateClient(new ManagedIdentityAuthService());
        }

        private static FhirClient CreateConfidentialApplicationClient()
        {
            return CreateClient(new OAuthConfidentialClientAuthService());
        }

        private class BearerTokenAuthorizationMessageHandler : HttpClientHandler
        {
            public BearerTokenAuthorizationMessageHandler(Uri uri, TokenCredential tokenCredentialProvider)
            {
                TokenCredential = EnsureArg.IsNotNull(tokenCredentialProvider, nameof(tokenCredentialProvider));
                Uri = EnsureArg.IsNotNull(uri);
                Scopes = new string[] { Uri + ".default" };
            }

            private TokenCredential TokenCredential { get; }

            private Uri Uri { get; }

            private string[] Scopes { get; }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var requestContext = new TokenRequestContext(Scopes);
                var accessToken = await TokenCredential.GetTokenAsync(requestContext, CancellationToken.None);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);
                var response = await base.SendAsync(request, cancellationToken);
                return response;
            }
        }
    }
}
