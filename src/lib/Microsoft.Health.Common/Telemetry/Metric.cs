// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Health.Common.Telemetry
{
    public class Metric
    {
        public Metric(string name, IDictionary<string, object> dimensions)
        {
            Name = name;
            Dimensions = dimensions;
        }

        public string Name { get; set; }

        public IDictionary<string, object> Dimensions { get; }

        public void AddPrefixToName(string prefix)
        {
            if (!Name.StartsWith(prefix, System.StringComparison.CurrentCulture))
            {
                Name = $"{prefix}{Name}";
            }
        }
    }
}
