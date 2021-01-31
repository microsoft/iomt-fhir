// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Common.Auth
{
    public interface IAzureCredential
    {
        Azure.Core.TokenCredential TokenCredential { get; set; }

        string ConnectionString { get; set; }
    }
}
