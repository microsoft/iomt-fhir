// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Microsoft.Health.Common.Auth;

namespace Microsoft.Health.Extensions.Host.Auth
{
    public class ManagedIdentityAuthService : TokenCredential
    {
        private TokenCredential _tokenCredential;

        public ManagedIdentityAuthService()
        {
            _tokenCredential = new DefaultAzureCredential();
        }

        public ManagedIdentityAuthService(IAzureCredentialProvider azureCredentialProvider)
        {
            _tokenCredential = azureCredentialProvider.GetCredential();
        }

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            return _tokenCredential.GetToken(requestContext, cancellationToken);
        }

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            return _tokenCredential.GetTokenAsync(requestContext, cancellationToken);
        }
    }
}
