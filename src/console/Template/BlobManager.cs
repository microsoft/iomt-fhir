// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Azure.Storage.Blobs;
using EnsureThat;
using System.IO;
using System.Text;

namespace Microsoft.Health.Fhir.Ingest.Console.Template
{
    public class BlobManager : ITemplateManager
    {
        private BlobContainerClient _blobContainer;

        public BlobManager(string connectionString, string blobContainerName)
        {
            EnsureArg.IsNotNull(connectionString);
            EnsureArg.IsNotNull(blobContainerName);

            _blobContainer = new BlobContainerClient(connectionString, blobContainerName);
        }
        public byte[] GetTemplate(string templateName)
        {
            EnsureArg.IsNotNull(templateName);

            var blockBlob = _blobContainer.GetBlobClient(templateName);
            var memoryStream = new MemoryStream();
            blockBlob.DownloadTo(memoryStream);
            byte[] itemContent = memoryStream.ToArray();
            return itemContent;
        }

        public string GetTemplateAsString(string templateName)
        {
            var templateBuffer = GetTemplate(templateName);
            string templateContent = Encoding.UTF8.GetString(templateBuffer, 0, templateBuffer.Length);
            return templateContent;
        }
    }
}
