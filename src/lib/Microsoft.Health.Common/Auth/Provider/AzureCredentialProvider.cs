// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Azure.Identity;

namespace Microsoft.Health.Common.Auth
{
    public class AzureCredentialProvider : IAzureCredentialProvider
    {
        private CredentialOptions _credentialOptions;

        public AzureCredentialProvider(ICredentialOptions options)
        {
            _credentialOptions = (CredentialOptions)options;
        }

        public IAzureCredential GetCredential()
        {
            return GetTokenCredential();
        }

        public AzureCredential GetTokenCredential()
        {
            if (_credentialOptions.ClientCertificateCredential)
            {
                throw new NotSupportedException();
            }
            else
            {
                var tokenCredential = new DefaultAzureCredential();
                return new AzureCredential(tokenCredential);
            }
        }
    }
}
