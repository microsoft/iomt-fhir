// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Azure.Identity;
using Azure.Storage.Blobs;
using EnsureThat;
using Microsoft.Health.Common.Auth;

namespace Microsoft.Health.Common.Storage
{
    public class BlobContainerClientFactory
    {
        public BlobContainerClient CreateStorageClient(BlobContainerClientOptions options, IAzureCredentialProvider provider = null)
        {
            EnsureArg.IsNotNull(options);
            var containerUri = EnsureArg.IsNotNull(options.BlobStorageContainerUri);
            var blobUri = new BlobUriBuilder(containerUri);

            if (options.ServiceManagedIdentityAuth)
            {
                var tokenCredential = new DefaultAzureCredential();
                return new BlobContainerClient(containerUri, tokenCredential);
            }
            else if (!string.IsNullOrEmpty(options.ConnectionString))
            {
                return new BlobContainerClient(containerUri.ToString(), blobUri.BlobContainerName);
            }
            else if (provider != null)
            {
                var tokenCredential = provider.GetCredential();
                return new BlobContainerClient(containerUri, tokenCredential);
            }
            else
            {
                throw new Exception($"Unable to create blob container client for {blobUri}");
            }
        }
    }
}
