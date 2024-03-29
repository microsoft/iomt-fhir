﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class IotJsonPathContentTemplate : JsonPathContentTemplate
    {
        [JsonIgnore]
        public override string DeviceIdExpression
        {
            get => "$.SystemProperties.iothub-connection-device-id"; set { }
        }

        [JsonIgnore]
        public override string TimestampExpression
        {
            get => "$.Properties.iothub-creation-time-utc"; set { }
        }

        [JsonIgnore]
        public virtual string AlternateTimestampExpression
        {
            get => "$.SystemProperties.iothub-enqueuedtime";
        }
    }
}
