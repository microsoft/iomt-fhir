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
            var containerUri = EnsureArg.IsNotNull(options.BlobStorageContainerUri, nameof(options.BlobStorageContainerUri));
            var blobUri = new BlobUriBuilder(containerUri);

            if (options.AuthenticationType == AuthenticationType.ManagedIdentity)
            {
                var tokenCredential = new DefaultAzureCredential();
                return new BlobContainerClient(containerUri, tokenCredential);
            }
            else if (options.AuthenticationType == AuthenticationType.ConnectionString)
            {
                EnsureArg.IsNotNull(options.ConnectionString, nameof(options.ConnectionString));
                EnsureArg.IsNotNull(blobUri.BlobContainerName);

                return new BlobContainerClient(options.ConnectionString, blobUri.BlobContainerName);
            }
            else if (options.AuthenticationType == AuthenticationType.Custom)
            {
                EnsureArg.IsNotNull(provider);

                var tokenCredential = provider.GetCredential();
                return new BlobContainerClient(containerUri, tokenCredential);
            }
            else
            {
                var ex = $"Unable to create blob container client for {blobUri}.";
                var message = "No authentication type was specified for BlobContainerClientOptions";
                throw new Exception($"{ex} {message}");
            }
        }
    }
}
