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
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Extensions.Fhir.Config;
using Microsoft.Health.Extensions.Fhir.Telemetry.Exceptions;
using Microsoft.Health.Extensions.Fhir.Telemetry.Metrics;
using Microsoft.Health.Logging.Telemetry;
using FhirClient = Microsoft.Health.Fhir.Client.FhirClient;
using IFhirClient = Microsoft.Health.Fhir.Client.IFhirClient;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class FhirClientFactory : IFactory<IFhirClient>
    {
        private readonly FhirClientFactoryOptions _options;
        private readonly ITelemetryLogger _logger;

        public FhirClientFactory(IOptions<FhirClientFactoryOptions> options, ITelemetryLogger logger)
        {
            EnsureArg.IsNotNull(options);
            EnsureArg.IsNotNull(logger);
            _options = options.Value;
            _logger = logger;
        }

        public IFhirClient Create()
        {
            EnsureArg.IsNotNull(_options.CredentialProvider);
            var tokenCredential = _options.CredentialProvider.GetCredential();
            return CreateCustomAuthClient(tokenCredential, _logger);
        }

#pragma warning disable CA2000
        private static IFhirClient CreateClient(TokenCredential tokenCredential, ITelemetryLogger logger)
        {
            EnsureArg.IsNotNull(tokenCredential, nameof(tokenCredential));

            var url = Environment.GetEnvironmentVariable("FhirService:Url");
            EnsureArg.IsNotNullOrEmpty(url, nameof(url));

            var httpMessageHandler = new BearerTokenAuthorizationMessageHandler(url, tokenCredential, logger)
            {
                CheckCertificateRevocationList = true,
            };

            var httpClient = new HttpClient(httpMessageHandler)
            {
                BaseAddress = new Uri(url),
                Timeout = FhirClientFactoryOptions.DefaultTimeout,
            };

            FhirClient client = null;
            try
            {
                client = new FhirClient(httpClient, ResourceFormat.Json);
            }
#pragma warning disable CA1031
            catch (Exception ex)
#pragma warning restore CA1031
            {
                FhirServiceExceptionProcessor.ProcessException(ex, logger);
            }

            return client;
        }
#pragma warning restore CA2000

        private static IFhirClient CreateCustomAuthClient(TokenCredential tokenCredential, ITelemetryLogger logger)
        {
            return CreateClient(tokenCredential, logger);
        }

        private class BearerTokenAuthorizationMessageHandler : HttpClientHandler
        {
            public BearerTokenAuthorizationMessageHandler(string url, TokenCredential tokenCredential, ITelemetryLogger logger)
            {
                TokenCredential = EnsureArg.IsNotNull(tokenCredential, nameof(tokenCredential));
                Logger = EnsureArg.IsNotNull(logger, nameof(logger));
                EnsureArg.IsNotNull(url, nameof(url));
                Uri = new Uri(url);
                Scopes = new string[] { Uri.ToString().EndsWith(@"/", StringComparison.InvariantCulture) ? Uri + ".default" : Uri + "/.default" };
            }

            private ITelemetryLogger Logger { get; }

            private TokenCredential TokenCredential { get; }

            private Uri Uri { get; }

            private string[] Scopes { get; }

            private AccessToken AccessToken { get; set; }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var requestContext = new TokenRequestContext(Scopes);

                if (string.IsNullOrEmpty(AccessToken.Token) || (AccessToken.ExpiresOn < DateTime.UtcNow.AddMinutes(1)))
                {
                    AccessToken = await TokenCredential.GetTokenAsync(requestContext, cancellationToken);
                }

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken.Token);

                var response = await base.SendAsync(request, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var statusDescription = response.ReasonPhrase.Replace(" ", string.Empty, StringComparison.InvariantCulture);
                    var severity = response.StatusCode == System.Net.HttpStatusCode.TooManyRequests ? ErrorSeverity.Informational : ErrorSeverity.Critical;
                    Logger.LogMetric(FhirClientMetrics.HandledException($"{ErrorType.FHIRServiceError}{statusDescription}", severity), 1);
                }

                return response;
            }
        }
    }
}
