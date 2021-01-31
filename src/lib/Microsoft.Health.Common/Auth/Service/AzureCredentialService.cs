// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Common.Auth
{
    public class AzureCredentialService : IAzureCredentialService
    {
        private IAzureCredential _credential;
        private IAzureCredentialProvider _azureCredentialProvider;

        public AzureCredentialService(IAzureCredentialProvider azureCredentialProvider)
        {
            _azureCredentialProvider = azureCredentialProvider;
        }

        public IAzureCredential GetCredential()
        {
            _credential = _azureCredentialProvider.GetCredential();
            return _credential;
        }
    }
}
