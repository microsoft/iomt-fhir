// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using EnsureThat;
using Microsoft.Health.Common.Telemetry;

namespace Microsoft.Health.Fhir.Ingest.Telemetry
{
    /// <summary>
    /// Defines known metrics and metric dimensions for use in Application Insights
    /// </summary>
    public static class IomtMetrics
    {
        private static string _nameDimension = DimensionNames.Name;
        private static string _categoryDimension = DimensionNames.Category;
        private static string _errorTypeDimension = DimensionNames.ErrorType;
        private static string _errorSeverityDimension = DimensionNames.ErrorSeverity;
        private static string _operationDimension = DimensionNames.Operation;

        private static Metric _measurementIngestionLatency = new Metric(
            "MeasurementIngestionLatency",
            new Dictionary<string, object>
            {
                { _nameDimension, "MeasurementIngestionLatency" },
                { _categoryDimension, Category.Latency },
                { _operationDimension, ConnectorOperation.FHIRConversion },
            });

        private static Metric _measurementIngestionLatencyMs = new Metric(
            "MeasurementIngestionLatencyMs",
            new Dictionary<string, object>
            {
                { _nameDimension, "MeasurementIngestionLatencyMs" },
                { _categoryDimension, Category.Latency },
                { _operationDimension, ConnectorOperation.FHIRConversion },
            });

        private static Metric _measurementGroup = new Metric(
            "MeasurementGroup",
            new Dictionary<string, object>
            {
                { _nameDimension, "MeasurementGroup" },
                { _categoryDimension, Category.Traffic },
                { _operationDimension, ConnectorOperation.FHIRConversion },
            });

        private static Metric _measurement = new Metric(
            "Measurement",
            new Dictionary<string, object>
            {
                { _nameDimension, "Measurement" },
                { _categoryDimension, Category.Traffic },
                { _operationDimension, ConnectorOperation.FHIRConversion },
            });

        private static Metric _deviceEvent = new Metric(
            "DeviceEvent",
            new Dictionary<string, object>
            {
                { _nameDimension, "DeviceEvent" },
                { _categoryDimension, Category.Traffic },
                { _operationDimension, ConnectorOperation.Normalization },
            });

        private static Metric _normalizedEvent = new Metric(
            "NormalizedEvent",
            new Dictionary<string, object>
            {
                { _nameDimension, "NormalizedEvent" },
                { _categoryDimension, Category.Traffic },
                { _operationDimension, ConnectorOperation.Normalization },
            });

        private static Metric _deviceEventProcessingLatency = new Metric(
            "DeviceEventProcessingLatency",
            new Dictionary<string, object>
            {
                { _nameDimension, "DeviceEventProcessingLatency" },
                { _categoryDimension, Category.Latency },
                { _operationDimension, ConnectorOperation.Normalization },
            });

        private static Metric _deviceEventProcessingLatencyMs = new Metric(
            "DeviceEventProcessingLatencyMs",
            new Dictionary<string, object>
            {
                { _nameDimension, "DeviceEventProcessingLatencyMs" },
                { _categoryDimension, Category.Latency },
                { _operationDimension, ConnectorOperation.Normalization },
            });

        private static Metric _deviceIngressSizeBytes = new Metric(
            "DeviceIngressSizeBytes",
            new Dictionary<string, object>
            {
                { _nameDimension, "DeviceIngressSizeBytes" },
                { _categoryDimension, Category.Traffic },
                { _operationDimension, ConnectorOperation.Normalization },
            });

        private static Metric _notSupported = new Metric(
            "NotSupportedException",
            new Dictionary<string, object>
            {
                { _nameDimension, "NotSupportedException" },
                { _categoryDimension, Category.Errors },
                { _errorTypeDimension, ErrorType.FHIRResourceError },
                { _errorSeverityDimension, ErrorSeverity.Warning },
                { _operationDimension, ConnectorOperation.FHIRConversion },
            });

        /// <summary>
        /// The latency between event ingestion and output to FHIR processor.
        /// </summary>
        public static Metric MeasurementIngestionLatency()
        {
            return _measurementIngestionLatency;
        }

        /// <summary>
        /// The latency between event ingestion and output to FHIR processor, in milliseconds.
        /// </summary>
        public static Metric MeasurementIngestionLatencyMs()
        {
            return _measurementIngestionLatencyMs;
        }

        /// <summary>
        /// The number of measurement groups generated by the FHIR processor based on provided input.
        /// </summary>
        public static Metric MeasurementGroup()
        {
            return _measurementGroup;
        }

        /// <summary>
        /// The number of measurement readings to import to FHIR.
        /// </summary>
        public static Metric Measurement()
        {
            return _measurement;
        }

        /// <summary>
        /// The number of input events received.
        /// </summary>
        public static Metric DeviceEvent()
        {
            return _deviceEvent;
        }

        /// <summary>
        /// The number of normalized events generated for further processing.
        /// </summary>
        public static Metric NormalizedEvent()
        {
            return _normalizedEvent;
        }

        /// <summary>
        /// The latency between the event ingestion time and normalization processing. An increase here indicates a backlog of messages to process.
        /// </summary>
        public static Metric DeviceEventProcessingLatency()
        {
            return _deviceEventProcessingLatency;
        }

        /// <summary>
        /// The latency between the event ingestion time and normalization processing, in milliseconds. An increase here indicates a backlog of messages to process.
        /// </summary>
        public static Metric DeviceEventProcessingLatencyMs()
        {
            return _deviceEventProcessingLatencyMs;
        }

        /// <summary>
        /// A metric that measures the amount of data (in bytes) ingested by normalization processing.
        /// </summary>
        public static Metric DeviceIngressSizeBytes()
        {
            return _deviceIngressSizeBytes;
        }

        /// <summary>
        /// A metric for when FHIR resource does not support the provided type as a value.
        /// </summary>
        public static Metric NotSupported()
        {
            return _notSupported;
        }

        public static Metric UnhandledException(string exceptionName, string connectorStage)
        {
            EnsureArg.IsNotNull(exceptionName);
            return new Metric(
                "UnhandledException",
                new Dictionary<string, object>
                {
                    { _nameDimension, exceptionName },
                    { _categoryDimension, Category.Errors },
                    { _errorTypeDimension, ErrorType.GeneralError },
                    { _errorSeverityDimension, ErrorSeverity.Critical },
                    { _operationDimension, connectorStage },
                });
        }

        public static Metric HandledException(string exceptionName, string connectorStage)
        {
            return new Metric(
                exceptionName,
                new Dictionary<string, object>
                {
                    { _nameDimension, exceptionName },
                    { _categoryDimension, Category.Errors },
                    { _errorTypeDimension, ErrorType.GeneralError },
                    { _errorSeverityDimension, ErrorSeverity.Critical },
                    { _operationDimension, connectorStage },
                });
        }
    }
}
