﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public abstract class Template : LineAwareJsonObject
    {
        [JsonProperty(Required = Required.Always)]
        public virtual string TypeName { get; set; }
    }
}
