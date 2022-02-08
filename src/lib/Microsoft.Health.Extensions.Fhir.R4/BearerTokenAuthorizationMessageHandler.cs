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
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Extensions.Fhir.Telemetry.Metrics;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Extensions.Fhir
{
    public class BearerTokenAuthorizationMessageHandler : DelegatingHandler
    {
        public BearerTokenAuthorizationMessageHandler(Uri uri, TokenCredential tokenCredentialProvider, ITelemetryLogger logger)
        {
            TokenCredential = EnsureArg.IsNotNull(tokenCredentialProvider, nameof(tokenCredentialProvider));
            Uri = EnsureArg.IsNotNull(uri, nameof(uri));
            Logger = EnsureArg.IsNotNull(logger, nameof(logger));
            Scopes = new string[] { Uri.ToString().EndsWith(@"/") ? Uri + ".default" : Uri + "/.default" };
        }

        private ITelemetryLogger Logger { get; }

        private TokenCredential TokenCredential { get; }

        private Uri Uri { get; }

        private string[] Scopes { get; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var requestContext = new TokenRequestContext(Scopes);
            var accessToken = await TokenCredential.GetTokenAsync(requestContext, cancellationToken);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);
            var response = await base.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var statusDescription = response.ReasonPhrase.Replace(" ", string.Empty);
                var severity = response.StatusCode == System.Net.HttpStatusCode.TooManyRequests ? ErrorSeverity.Informational : ErrorSeverity.Critical;
                Logger.LogMetric(FhirClientMetrics.HandledException($"{ErrorType.FHIRServiceError}{statusDescription}", severity), 1);
            }

            return response;
        }
    }
}
