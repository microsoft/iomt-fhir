// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.IO;
using Azure.Storage.Blobs;
using EnsureThat;

namespace Microsoft.Health.Events.Repository
{
    public class StorageManager : IRepositoryManager
    {
        private BlobContainerClient _blobContainer;

        public StorageManager(Uri containerUri, BlobContainerClient containerClient)
        {
            EnsureArg.IsNotNull(containerUri);
            _blobContainer = containerClient;
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
