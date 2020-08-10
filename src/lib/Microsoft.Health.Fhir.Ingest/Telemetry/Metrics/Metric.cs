// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Health.Fhir.Ingest.Telemetry.Metrics
{
    public class Metric
    {
        private string _name;

        private IDictionary<string, object> _dimensions;

        public Metric(string name, IDictionary<string, object> dimensions)
        {
            _name = name;
            _dimensions = dimensions;
        }

        public string Name => _name;

        public IDictionary<string, object> Dimensions => _dimensions;
    }
}
