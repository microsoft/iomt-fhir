// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.IO;
using Azure.Storage.Blobs;
using EnsureThat;
using Microsoft.Health.Common.Auth;

namespace Microsoft.Health.Events.Repository
{
    public class StorageManager : IRepositoryManager
    {
        private BlobContainerClient _blobContainer;

        public StorageManager(Uri containerUri, IAzureCredentialService credentialService)
        {
            EnsureArg.IsNotNull(containerUri);
            _blobContainer = CreateStorageClient(credentialService, containerUri);
        }

        public static BlobContainerClient CreateStorageClient(IAzureCredentialService credentialService, Uri containerUri)
        {
            EnsureArg.IsNotNull(credentialService);

            var blobUri = new BlobUriBuilder(containerUri);
            var tokenCredential = credentialService.GetCredential().TokenCredential;
            var connectionString = credentialService.GetCredential().ConnectionString;

            if (tokenCredential != null)
            {
                return new BlobContainerClient(containerUri, tokenCredential);
            }
            else if (!string.IsNullOrWhiteSpace(connectionString))
            {
                return new BlobContainerClient(connectionString, blobUri.BlobContainerName);
            }
            else
            {
                var ex = new Exception($"Unable to create blob container client for {blobUri}");
                throw ex;
            }
        }

        public byte[] GetItem(string itemName)
        {
            EnsureArg.IsNotNull(itemName);

            var blockBlob = _blobContainer.GetBlobClient(itemName);

            using (var memoryStream = new MemoryStream())
            {
                blockBlob.DownloadTo(memoryStream);
                byte[] itemContent = memoryStream.ToArray();
                return itemContent;
            }
        }
    }
}
