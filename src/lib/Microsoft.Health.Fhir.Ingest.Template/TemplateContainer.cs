﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Fhir.Ingest.Template.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    [JsonConverter(typeof(TemplateContainerJsonConverter))]
    public class TemplateContainer : LineAwareJsonObject
    {
        public string TemplateType { get; set; }

        public JToken Template { get; set; }

        public bool MatchTemplateName(string expected)
        {
            const string TemplateSuffix = "Template";
            return StringComparer.InvariantCultureIgnoreCase.Equals(expected, TemplateType)
                || StringComparer.InvariantCultureIgnoreCase.Equals(expected, TemplateType + TemplateSuffix);
        }
    }
}
