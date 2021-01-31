// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Azure.Core;

namespace Microsoft.Health.Common.Auth
{
    public class AzureCredential : IAzureCredential
    {
        public AzureCredential(TokenCredential tokenCredential)
        {
           TokenCredential = tokenCredential;
        }

        public AzureCredential(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public TokenCredential TokenCredential { get; set; }

        public string ConnectionString { get; set; }
    }
}
