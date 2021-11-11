// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Common.Telemetry;

namespace Microsoft.Health.Events.Telemetry
{
    /// <summary>
    /// Defines known metrics and metric dimensions for use in Application Insights
    /// </summary>
    public static class EventMetrics
    {
        private static readonly string _nameDimension = DimensionNames.Name;
        private static readonly string _categoryDimension = DimensionNames.Category;
        private static readonly string _partitionDimension = DimensionNames.Identifier;
        private static readonly string _errorTypeDimension = DimensionNames.ErrorType;
        private static readonly string _errorSeverityDimension = DimensionNames.ErrorSeverity;
        private static readonly string _operationDimension = DimensionNames.Operation;
        private static readonly string _reasonDimension = DimensionNames.Reason;

        private static string _connectorOperation = ConnectorOperation.Unknown;

        public static void SetConnectorOperation(string connectorOperation)
        {
            if (_connectorOperation != ConnectorOperation.Unknown)
            {
                throw new InvalidOperationException($"Connector operation can only be assinged once. Current value - {_connectorOperation}");
            }

            _connectorOperation = connectorOperation;
        }

        /// <summary>
        /// Signals that an event hub was changed.
        /// </summary>
        /// <param name="eventHubName">The name of the event hub</param>
        public static Metric EventHubChanged(string eventHubName)
        {
            return EventMetricDefinition.EventHubChanged
                .CreateBaseMetric(Category.Traffic, _connectorOperation)
                .AddDimension(_reasonDimension, eventHubName);
        }

        /// <summary>
        /// Signals that an event hub partition has been intialized.
        /// </summary>
        /// <param name="partitionId">The partition id of the event hub</param>
        public static Metric EventHubPartitionInitialized(string partitionId)
        {
            return EventMetricDefinition.EventHubPartitionInitialized
                .CreateBaseMetric(Category.Traffic, _connectorOperation)
                .AddDimension(_partitionDimension, partitionId);
        }

        /// <summary>
        /// Signals that a batch of event hub events was flushed.
        /// </summary>
        /// <param name="partitionId">The partition id of the event hub</param>
        public static Metric EventsFlushed(string partitionId)
        {
            return EventMetricDefinition.EventsFlushed
                .CreateBaseMetric(Category.Traffic, _connectorOperation)
                .AddDimension(_partitionDimension, partitionId);
        }

        /// <summary>
        /// Signals that a batch of event hub events was consumed downstream.
        /// </summary>
        /// <param name="eventMetricDefinition">The metric definition that contains a metric name for the metric emitted after events are consumed</param>\
        public static Metric EventsConsumed(EventMetricDefinition eventMetricDefinition)
        {
            return eventMetricDefinition
                .CreateBaseMetric(Category.Traffic, _connectorOperation);
        }

        /// <summary>
        /// Signals that a new watermark was published for a partition.
        /// </summary>
        /// <param name="partitionId">The partition id of the event hub</param>
        public static Metric EventWatermark(string partitionId)
        {
            return EventMetricDefinition.EventsWatermarkUpdated
                .CreateBaseMetric(Category.Latency, _connectorOperation)
                .AddDimension(_partitionDimension, partitionId);
        }

        /// <summary>
        /// Signals the timestamp corresponding to the last processed event per partition.
        /// </summary>
        /// <param name="partitionId">The partition id of the event hub</param>
        /// <param name="triggerReason">The trigger that caused the events to be flushed and processed </param>
        public static Metric EventTimestampLastProcessedPerPartition(string partitionId, string triggerReason)
        {
            return EventMetricDefinition.EventTimestampLastProcessedPerPartition
                .CreateBaseMetric(Category.Latency, _connectorOperation)
                .AddDimension(_partitionDimension, partitionId)
                .AddDimension(_reasonDimension, triggerReason);
        }

        /// <summary>
        /// A metric recorded when there is an error reading from or connecting with an Event Hub.
        /// </summary>
        /// <param name="exceptionName">The name of the exception</param>
        /// <param name="connectorStage">The stage of the connector</param>
        public static Metric HandledException(string exceptionName, string connectorStage)
        {
            return exceptionName.ToErrorMetric(connectorStage, ErrorType.EventHubError, ErrorSeverity.Critical);
        }
    }
}
