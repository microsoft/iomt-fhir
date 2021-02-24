// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Tools.DataMapper.Models
{
    public class TransformationTestResponse
    {
        public string Result { get; set; }

        public string Reason { get; set; }

        public string FhirData { get; set; }
    }
}
