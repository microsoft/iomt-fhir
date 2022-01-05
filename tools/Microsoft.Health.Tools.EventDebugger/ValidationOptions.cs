// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.IO;

namespace Microsoft.Health.Tools.EventDebugger
{
    public class ValidationOptions
    {
        public FileInfo DeviceMapping { get; set; }

        public FileInfo FhirMapping { get; set; }

        public FileInfo DeviceData { get; set; }
    }
}