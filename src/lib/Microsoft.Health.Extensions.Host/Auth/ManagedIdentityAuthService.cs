// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;

namespace Microsoft.Health.Extensions.Host.Auth
{
    public class ManagedIdentityAuthService : IAuthService
    {
        public async Task<string> GetAccessTokenAsync()
        {
            var resource = System.Environment.GetEnvironmentVariable("FhirService:Resource");
            var tokenProvider = new AzureServiceTokenProvider();
            return await tokenProvider.GetAccessTokenAsync(resource).ConfigureAwait(false);
        }
    }
}
