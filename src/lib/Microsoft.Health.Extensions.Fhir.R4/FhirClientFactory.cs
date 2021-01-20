// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Hl7.Fhir.Rest;
using Microsoft.Extensions.Options;
using Microsoft.Health.Common;
using Microsoft.Health.Extensions.Fhir.Config;
using Microsoft.Health.Extensions.Host.Auth;

namespace Microsoft.Health.Extensions.Fhir
{
    public class FhirClientFactory : IFactory<FhirClient>
    {
        private readonly bool _useManagedIdentity = false;

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

        public static IFactory<FhirClient> Instance { get; } = new FhirClientFactory();

        public FhirClient Create()
        {
            return _useManagedIdentity ? CreateManagedIdentityClient() : CreateConfidentialApplicationClient();
        }

        private static FhirClient CreateClient(IAuthService authService)
        {
            var url = System.Environment.GetEnvironmentVariable("FhirService:Url");
            EnsureArg.IsNotNullOrEmpty(url, nameof(url));

            EnsureArg.IsNotNull(authService, nameof(authService));

            var fhirClientSettings = new FhirClientSettings
            {
                PreferredFormat = ResourceFormat.Json,
            };

            var client = new FhirClient(url, fhirClientSettings, new BearerTokenAuthorizationMessageHandler(authService));

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
            public BearerTokenAuthorizationMessageHandler(IAuthService authService)
            {
                AuthService = EnsureArg.IsNotNull(authService, nameof(authService));
            }

            private IAuthService AuthService { get; }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var token = await AuthService.GetAccessTokenAsync();
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                return await base.SendAsync(request, cancellationToken);
            }
        }
    }
}
