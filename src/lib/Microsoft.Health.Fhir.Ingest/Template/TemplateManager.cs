// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EnsureThat;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class TemplateManager : ITemplateManager
    {
        public TemplateManager(BlobContainerClient blobContainer)
        {
            BlobContainer = EnsureArg.IsNotNull(blobContainer, nameof(blobContainer));
        }

        protected BlobContainerClient BlobContainer { get; }

        public byte[] GetTemplate(string templateName)
        {
            return GetBlobContent(templateName);
        }

        public string GetTemplateAsString(string templateName)
        {
            var templateBuffer = GetTemplate(templateName);
            string templateContent = Encoding.UTF8.GetString(templateBuffer, 0, templateBuffer.Length);
            return templateContent;
        }

        protected byte[] GetBlobContent(string itemName)
        {
            EnsureArg.IsNotNull(itemName);

            var blockBlob = BlobContainer.GetBlobClient(itemName);

            using var memoryStream = new MemoryStream();
            blockBlob.DownloadTo(memoryStream);
            byte[] itemContent = memoryStream.ToArray();
            return itemContent;
        }

        public async Task<string> GetTemplateContentIfChangedSince(string templateName, DateTimeOffset contentTimestamp = default, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrWhiteSpace(templateName, nameof(templateName));

            var blobClient = BlobContainer.GetBlobClient(templateName);
            BlobRequestConditions conditions = new () { IfModifiedSince = contentTimestamp };

            using var ms = new MemoryStream();
            await blobClient.DownloadToAsync(ms, conditions: conditions, cancellationToken: cancellationToken);

            if (ms.Length == 0)
            {
                // No new content found, return null.
                return null;
            }

            return Encoding.UTF8.GetString(ms.ToArray());
        }
    }
}
