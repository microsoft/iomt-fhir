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
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Extensions.Fhir.Config;
using Microsoft.Health.Extensions.Fhir.Telemetry.Exceptions;
using Microsoft.Health.Extensions.Fhir.Telemetry.Metrics;
using Microsoft.Health.Extensions.Host.Auth;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Extensions.Fhir
{
    public class FhirClientFactory : IFactory<FhirClient>
    {
        private readonly bool _useManagedIdentity = false;
        private readonly IAzureCredentialProvider _tokenCredentialProvider;
        private readonly ITelemetryLogger _logger;

        public FhirClientFactory(IOptions<FhirClientFactoryOptions> options, ITelemetryLogger logger)
            : this(EnsureArg.IsNotNull(options, nameof(options)).Value.UseManagedIdentity, logger)
        {
        }

        private FhirClientFactory()
            : this(useManagedIdentity: false, logger: null)
        {
        }

        private FhirClientFactory(bool useManagedIdentity, ITelemetryLogger logger)
        {
            _useManagedIdentity = useManagedIdentity;
            _logger = logger;
        }

        public FhirClientFactory(IAzureCredentialProvider provider, ITelemetryLogger logger)
        {
            _tokenCredentialProvider = provider;
            _logger = logger;
        }

        public static IFactory<FhirClient> Instance { get; } = new FhirClientFactory();

        public FhirClient Create()
        {
            if (_tokenCredentialProvider != null)
            {
                return CreateClient(_tokenCredentialProvider.GetCredential(), _logger);
            }

            return _useManagedIdentity ? CreateManagedIdentityClient(_logger) : CreateConfidentialApplicationClient(_logger);
        }

        private static FhirClient CreateClient(TokenCredential tokenCredential, ITelemetryLogger logger)
        {
            EnsureArg.IsNotNull(tokenCredential, nameof(tokenCredential));

            var url = Environment.GetEnvironmentVariable("FhirService:Url");
            EnsureArg.IsNotNullOrEmpty(url, nameof(url));
            var uri = new Uri(url);

            var fhirClientSettings = new FhirClientSettings
            {
                PreferredFormat = ResourceFormat.Json,
            };

            FhirClient client = null;
            try
            {
                client = new FhirClient(url, fhirClientSettings, new BearerTokenAuthorizationMessageHandler(uri, tokenCredential, logger));
                FhirServiceValidator.ValidateFhirService(client, logger);
            }
            catch (Exception ex)
            {
                FhirServiceExceptionProcessor.ProcessException(ex, logger);
            }

            return client;
        }

        private static FhirClient CreateManagedIdentityClient(ITelemetryLogger logger)
        {
            return CreateClient(new ManagedIdentityAuthService(), logger);
        }

        private static FhirClient CreateConfidentialApplicationClient(ITelemetryLogger logger)
        {
            return CreateClient(new OAuthConfidentialClientAuthService(), logger);
        }

        private class BearerTokenAuthorizationMessageHandler : HttpClientHandler
        {
            public BearerTokenAuthorizationMessageHandler(Uri uri, TokenCredential tokenCredentialProvider, ITelemetryLogger logger)
            {
                TokenCredential = EnsureArg.IsNotNull(tokenCredentialProvider, nameof(tokenCredentialProvider));
                Uri = EnsureArg.IsNotNull(uri);
                Scopes = new string[] { Uri + ".default" };
                Logger = logger;
            }

            private ITelemetryLogger Logger { get; }

            private TokenCredential TokenCredential { get; }

            private Uri Uri { get; }

            private string[] Scopes { get; }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var requestContext = new TokenRequestContext(Scopes);
                var accessToken = await TokenCredential.GetTokenAsync(requestContext, CancellationToken.None);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);
                var response = await base.SendAsync(request, cancellationToken);

                if (Logger != null && !response.IsSuccessStatusCode)
                {
                    var statusDescription = response.ReasonPhrase.Replace(" ", string.Empty);
                    var severity = response.StatusCode == System.Net.HttpStatusCode.TooManyRequests ? ErrorSeverity.Informational : ErrorSeverity.Critical;
                    Logger.LogMetric(FhirClientMetrics.HandledException($"{ErrorType.FHIRServiceError}{statusDescription}", severity), 1);
                }

                return response;
            }
        }
    }
}
