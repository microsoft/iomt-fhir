// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class LineInfo : IJsonLineInfo
    {
        [JsonIgnore]
        public int LineNumber { get; set; }

        [JsonIgnore]
        public int LinePosition { get; set; }

        public bool HasLineInfo()
        {
            return true;
        }
    }
}