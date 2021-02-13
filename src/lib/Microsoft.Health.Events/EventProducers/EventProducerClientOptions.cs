// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Events.EventProducers
{
    public class EventProducerClientOptions
    {
        public string EventHubNamespaceFQDN { get; set; }

        public string EventHubConsumerGroup { get; set; }

        public string EventHubName { get; set; }

        public string ConnectionString { get; set; }

        public bool ServiceManagedIdentityAuth { get; set; }

        public bool CustomizedAuth { get; set; }
    }
}
