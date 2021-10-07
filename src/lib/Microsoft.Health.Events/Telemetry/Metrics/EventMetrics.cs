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

        private static Metric _eventHubPartitionInitialized = new Metric(
            "EventHubPartitionInitialized",
            new Dictionary<string, object>
            {
                { _nameDimension, "EventHubPartitionInitialized" },
                { _categoryDimension, Category.Traffic },
            });

        private static Metric _eventBatchCreated = new Metric(
            "EventBatchCreated",
            new Dictionary<string, object>
            {
                { _nameDimension, "EventBatchCreated" },
                { _categoryDimension, Category.Traffic },
            });

        private static Metric _eventsFlushed = new Metric(
            "EventsFlushed",
            new Dictionary<string, object>
            {
                { _nameDimension, "EventsFlushed" },
                { _categoryDimension, Category.Traffic },
            });

        private static Metric _eventsConsumed = new Metric(
            "EventsConsumed",
            new Dictionary<string, object>
            {
                { _nameDimension, "EventsConsumed" },
                { _categoryDimension, Category.Traffic },
            });

        public static string DeviceIngressSizeBytes { get; } = "DeviceIngressSizeBytes";

        public static string MeasurementToFhirBytes { get; } = "MeasurementToFhirBytes";

        /// <summary>
        /// Signals that an event hub partition has been intialized.
        /// </summary>
        public static Metric EventHubPartitionInitialized()
        {
            return _eventHubPartitionInitialized;
        }

        /// <summary>
        /// Signals that a batch of event hub events was created.
        /// </summary>
        public static Metric EventBatchCreated()
        {
            return _eventBatchCreated;
        }

        /// <summary>
        /// Signals that a batch of event hub events was flushed.
        /// </summary>
        public static Metric EventsFlushed()
        {
            return _eventsFlushed;
        }

        /// <summary>
        /// Signals that a batch of event hub events was consumed downstream.
        /// </summary>
        /// <param name="eventsConsumedLabel">The label that will be given to the metric emitted after the events are consumed</param>
        /// <param name="connectorStage">The stage of the IoT Connector</param>
        public static Metric EventsConsumed(string eventsConsumedLabel, string connectorStage)
        {
            return new Metric(
                eventsConsumedLabel,
                new Dictionary<string, object>
                {
                    { _nameDimension, eventsConsumedLabel },
                    { _categoryDimension, Category.Traffic },
                    { _operationDimension, connectorStage },
                });
        }

        /// <summary>
        /// Signals that a new watermark was published for a partition.
        /// </summary>
        /// <param name="partitionId">The partition id of the event hub</param>
        /// <param name="dateTime">The datetime of the watermark</param>
        public static Metric EventWatermark(string partitionId, DateTime dateTime)
        {
            return new Metric(
                "EventsWatermarkUpdated",
                new Dictionary<string, object>
                {
                    { _nameDimension, "EventsWatermarkUpdated" },
                    { _timeDimension, dateTime.ToString() },
                    { _partitionDimension, partitionId },
                    { _categoryDimension, Category.Latency },
                });
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
    }
}
