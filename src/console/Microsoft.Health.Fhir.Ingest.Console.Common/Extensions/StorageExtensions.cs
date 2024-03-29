﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Health.Common.Auth;
using Microsoft.Health.Common.Storage;
using Microsoft.Health.Events.EventCheckpointing;
using Microsoft.Health.Fhir.Ingest.Template;

namespace Microsoft.Health.Fhir.Ingest.Console.Common.Extensions
{
    public static class StorageExtensions
    {
        public static IServiceCollection AddStorageClient(this IServiceCollection services, IConfiguration config, string applicationType)
        {
            services.AddSingleton<BlobContainerClientFactory>();

            services.AddSingleton((sp) =>
            {
                // Add StorageCheckpointOptions
                var storageOptions = new StorageCheckpointOptions();
                config.GetSection(StorageCheckpointOptions.Settings).Bind(storageOptions);
                storageOptions.BlobPrefix = $"{applicationType}/{storageOptions.BlobPrefix}";
                return storageOptions;
            });
            services.AddSingleton((sp) =>
            {
                // Add BlobContainerOptions
                var checkpointContainerOptions = new BlobContainerClientOptions();
                config.GetSection("CheckpointStorage").Bind(checkpointContainerOptions);
                return checkpointContainerOptions;
            });
            services.AddSingleton((sp) =>
            {
                // Add BlobContainerClient
                var factory = sp.GetRequiredService<BlobContainerClientFactory>();
                var checkpointContainerOptions = sp.GetRequiredService<BlobContainerClientOptions>();
                var tokenProvider = sp.GetService<IAzureCredentialProvider>();
                return factory.CreateStorageClient(checkpointContainerOptions, tokenProvider);
            });

            return services;
        }

        public static IServiceCollection AddTemplateManager(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<ITemplateManager>((sp) =>
            {
                var tokenProvider = sp.GetService<IAzureCredentialProvider>();
                var blobClientFactory = sp.GetRequiredService<BlobContainerClientFactory>();
                var containerOptions = new BlobContainerClientOptions();
                config.GetSection("TemplateStorage").Bind(containerOptions);
                var containerClient = blobClientFactory.CreateStorageClient(containerOptions, tokenProvider);
                return new TemplateManager(containerClient);
            });

            return services;
        }
    }
}
