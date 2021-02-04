// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Health.Common.Storage
{
    public class BlobContainerClientOptions
    {
        public Uri BlobStorageContainerUri { get; set; }

        public string ConnectionString { get; set; }

        public bool ServiceManagedIdentityAuth { get; set; }

        public bool CustomizedAuth { get; set; }
    }
}
