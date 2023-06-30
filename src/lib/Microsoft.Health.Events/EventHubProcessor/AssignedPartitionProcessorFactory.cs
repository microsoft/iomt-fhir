// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Azure.Core;
using Azure.Messaging.EventHubs.Primitives;
using Microsoft.Health.Events.EventCheckpointing;
using Microsoft.Health.Events.EventConsumers.Service;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Events.EventHubProcessor
{
    public class AssignedPartitionProcessorFactory : IAssignedPartitionProcessorFactory
    {
        public AssignedPartitionProcessor CreateAssignedPartitionProcessor(
            IEventConsumerService eventConsumerService,
            ICheckpointClient checkpointClient,
            ITelemetryLogger logger,
            string[] assignedPartitions,
            int eventBatchMaximumCount,
            string consumerGroup,
            TokenCredential tokenCredential,
            string eventHubName,
            string fullyQualifiedNamespace,
            EventProcessorOptions clientOptions = default)
        {
            return new AssignedPartitionProcessor(
                eventConsumerService,
                checkpointClient,
                logger,
                assignedPartitions,
                eventBatchMaximumCount,
                consumerGroup,
                tokenCredential,
                eventHubName,
                fullyQualifiedNamespace,
                clientOptions);
        }
    }
}
