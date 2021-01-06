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
        public static Metric EventsConsumed()
        {
            return _eventsConsumed;
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
    }
}
