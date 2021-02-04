// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Azure.Core;
using Azure.Identity;

namespace Microsoft.Health.Common.Auth
{
    public class AzureCredentialProvider : IAzureCredentialProvider
    {
        public TokenCredential GetCredential()
        {
            return new DefaultAzureCredential();
        }
    }
}
