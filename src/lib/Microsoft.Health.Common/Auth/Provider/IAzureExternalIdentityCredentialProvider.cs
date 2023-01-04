// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Common.Auth
{
    // needed to differentiate cases where there are more than one IAzureCredentialProvider
    public interface IAzureExternalIdentityCredentialProvider : IAzureCredentialProvider
    {
    }
}
