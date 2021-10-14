// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Health.Common.Telemetry;

namespace Microsoft.Health.Events.Telemetry
{
    /// <summary>
    /// Defines known metrics and metric dimensions for use in Application Insights
    /// </summary>
    public static class EventMetrics
    {
        private static string _nameDimension = DimensionNames.Name;
        private static string _categoryDimension = DimensionNames.Category;
        private static string _timeDimension = DimensionNames.Timestamp;
        private static string _partitionDimension = DimensionNames.Identifier;
        private static string _errorTypeDimension = DimensionNames.ErrorType;
        private static string _errorSeverityDimension = DimensionNames.ErrorSeverity;
        private static string _operationDimension = DimensionNames.Operation;
        private static string _reasonDimension = DimensionNames.Reason;

        public static string ConnectorOperation { get; set; } = Health.Common.Telemetry.ConnectorOperation.Unknown;

        /// <summary>
        /// Signals that an event hub partition has been intialized.
        /// </summary>
        /// <param name="partitionId">The partition id of the event hub</param>
        public static Metric EventHubPartitionInitialized(string partitionId)
        {
            return CreateBaseEventMetric(EventMetricNames.EventHubPartitionInitialized, partitionId, Category.Traffic);
        }

        /// <summary>
        /// Signals that a batch of event hub events was flushed.
        /// </summary>
        /// <param name="partitionId">The partition id of the event hub</param>
        public static Metric EventsFlushed(string partitionId)
        {
            return CreateBaseEventMetric(EventMetricNames.EventsFlushed, partitionId, Category.Traffic);
        }

        /// <summary>
        /// Signals that a batch of event hub events was consumed downstream.
        /// </summary>
        /// <param name="eventMetricDefinition">The metric definition that contains a metric name for the metric emitted after events are consumed</param>\
        public static Metric EventsConsumed(EventMetricDefinition eventMetricDefinition)
        {
            var metricName = eventMetricDefinition.ToString();
            return new Metric(
                metricName,
                new Dictionary<string, object>
                {
                    { _nameDimension, metricName },
                    { _categoryDimension, Category.Traffic },
                    { _operationDimension, ConnectorOperation },
                });
        }

        /// <summary>
        /// Signals that a new watermark was published for a partition.
        /// </summary>
        /// <param name="partitionId">The partition id of the event hub</param>
        /// <param name="dateTime">The datetime of the watermark</param>
        public static Metric EventWatermark(string partitionId, DateTime dateTime)
        {
            return CreateBaseEventMetric(EventMetricNames.EventsWatermarkUpdated, partitionId, Category.Traffic)
                .AddDimension(_timeDimension, dateTime.ToString());
        }

        /// <summary>
        /// Signals the timestamp corresponding to the last processed event per partition.
        /// </summary>
        /// <param name="partitionId">The partition id of the event hub</param>
        /// <param name="triggerReason">The trigger that caused the events to be flushed and processed </param>
        public static Metric EventTimestampLastProcessedPerPartition(string partitionId, string triggerReason)
        {
            return CreateBaseEventMetric(EventMetricNames.EventTimestampLastProcessedPerPartition, partitionId, Category.Latency)
                .AddDimension(_reasonDimension, triggerReason);
        }

        /// <summary>
        /// A metric recorded when there is an error reading from or connecting with an Event Hub.
        /// </summary>
        /// <param name="exceptionName">The name of the exception</param>
        /// <param name="connectorStage">The stage of the connector</param>
        public static Metric HandledException(string exceptionName, string connectorStage)
        {
            return new Metric(
                exceptionName,
                new Dictionary<string, object>
                {
                    { _nameDimension, exceptionName },
                    { _categoryDimension, Category.Errors },
                    { _errorTypeDimension, ErrorType.EventHubError },
                    { _errorSeverityDimension, ErrorSeverity.Critical },
                    { _operationDimension, connectorStage },
                });
        }

        private static Metric CreateBaseEventMetric(EventMetricNames metricName, string partitionId, string category)
        {
            var metricNameString = metricName.ToString();
            return new Metric(
                metricNameString,
                new Dictionary<string, object>
                {
                    { _nameDimension, metricNameString },
                    { _partitionDimension, partitionId },
                    { _categoryDimension, category },
                    { _operationDimension, ConnectorOperation },
                });
        }
    }
}
