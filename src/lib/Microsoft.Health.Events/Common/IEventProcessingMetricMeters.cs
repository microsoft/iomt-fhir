// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Events.Model;

namespace Microsoft.Health.Events.Common
{
    public interface IEventProcessingMetricMeters
    {
        Task<IEnumerable<KeyValuePair<Metric, double>>> GetMetrics(IEnumerable<IEventMessage> events);
    }
}
