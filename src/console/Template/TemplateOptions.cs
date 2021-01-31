// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------


using System;

namespace Microsoft.Health.Fhir.Ingest.Console.Storage
{
    public class TemplateOptions
    {
        public const string Settings = "TemplateStorage";

        public Uri BlobStorageContainerUri { get; set; }
    }
}
