// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using EnsureThat;
using Microsoft.Health.Common.Auth;
using Microsoft.Identity.Client;

namespace Microsoft.Health.Extensions.Host.Auth
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/dotnet/api/overview/azure/app-auth-migration
    /// </summary>
    public class OAuthConfidentialClientAuthService : TokenCredential, IFhirTokenProvider
    {
        public static async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
        {
            var authResult = await AquireServiceTokenAsync(cancellationToken).ConfigureAwait(false);
            return authResult.AccessToken;
        }

        private static async Task<AuthenticationResult> AquireServiceTokenAsync(CancellationToken cancellationToken)
        {
            var resource = System.Environment.GetEnvironmentVariable("FhirService:Resource");
            var authority = System.Environment.GetEnvironmentVariable("FhirService:Authority");
            var clientId = System.Environment.GetEnvironmentVariable("FhirService:ClientId");
            var clientSecret = System.Environment.GetEnvironmentVariable("FhirService:ClientSecret");

            EnsureArg.IsNotNullOrEmpty(resource, nameof(resource));
            EnsureArg.IsNotNullOrEmpty(authority, nameof(authority));
            EnsureArg.IsNotNullOrEmpty(clientId, nameof(clientId));
            EnsureArg.IsNotNullOrEmpty(clientSecret, nameof(clientSecret));

            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(clientId).WithAuthority(authority).WithClientSecret(clientSecret).Build();

            return await app.AcquireTokenForClient(new List<string>() { resource }).ExecuteAsync(cancellationToken);
        }

        public async override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            var authResult = await AquireServiceTokenAsync(cancellationToken).ConfigureAwait(false);
            var accessToken = new AccessToken(authResult.AccessToken, authResult.ExpiresOn);
            return accessToken;
        }

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            ValueTask<AccessToken> valueTask = Task.Run(() => GetTokenAsync(requestContext, cancellationToken)).GetAwaiter().GetResult();
            return valueTask.Result;
        }

        public TokenCredential GetTokenCredential()
        {
            return this;
        }
    }
}