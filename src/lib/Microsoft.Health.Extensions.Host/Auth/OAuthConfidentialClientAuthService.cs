// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using EnsureThat;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Microsoft.Health.Extensions.Host.Auth
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/dotnet/api/overview/azure/app-auth-migration
    /// </summary>
    public class OAuthConfidentialClientAuthService : TokenCredential
    {
        public static async Task<string> GetAccessTokenAsync()
        {
            var authResult = await AquireServiceTokenAsync().ConfigureAwait(false);
            return authResult.AccessToken;
        }

        private static async Task<AuthenticationResult> AquireServiceTokenAsync()
        {
            var resource = System.Environment.GetEnvironmentVariable("FhirService:Resource");
            var authority = System.Environment.GetEnvironmentVariable("FhirService:Authority");
            var clientId = System.Environment.GetEnvironmentVariable("FhirService:ClientId");
            var clientSecret = System.Environment.GetEnvironmentVariable("FhirService:ClientSecret");

            EnsureArg.IsNotNullOrEmpty(resource, nameof(resource));
            EnsureArg.IsNotNullOrEmpty(authority, nameof(authority));
            EnsureArg.IsNotNullOrEmpty(clientId, nameof(clientId));
            EnsureArg.IsNotNullOrEmpty(clientSecret, nameof(clientSecret));

            var authContext = new AuthenticationContext(authority);
            var clientCredential = new ClientCredential(clientId, clientSecret);

            try
            {
                return await authContext.AcquireTokenSilentAsync(resource, clientCredential, UserIdentifier.AnyUser).ConfigureAwait(false);
            }
            catch (AdalException adalEx)
            {
                switch (adalEx.ErrorCode)
                {
                    case AdalError.FailedToAcquireTokenSilently:
                        {
                            return await authContext.AcquireTokenAsync(resource, clientCredential).ConfigureAwait(false);
                        }

                    default: throw;
                }
            }
        }

        public async override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            var authResult = await AquireServiceTokenAsync().ConfigureAwait(false);
            var accessToken = new AccessToken(authResult.AccessToken, authResult.ExpiresOn);
            return accessToken;
        }

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            ValueTask<AccessToken> valueTask = Task.Run(() => GetTokenAsync(requestContext, cancellationToken)).GetAwaiter().GetResult();
            return valueTask.Result;
        }
    }
}