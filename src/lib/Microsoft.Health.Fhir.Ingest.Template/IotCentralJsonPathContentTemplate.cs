// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class IotCentralJsonPathContentTemplate : JsonPathContentTemplate
    {
        [JsonIgnore]
        public override string DeviceIdExpression
        {
            get => "$.deviceId"; set { }
        }

        [JsonIgnore]
        public override string TimestampExpression
        {
            get => "$.enqueuedTime"; set { }
        }
    }
}
