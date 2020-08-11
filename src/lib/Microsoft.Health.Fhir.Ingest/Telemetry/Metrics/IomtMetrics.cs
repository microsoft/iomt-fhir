// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using EnsureThat;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Telemetry.Dimensions;
using Microsoft.Health.Fhir.Ingest.Telemetry.Metrics;

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
        private static string _stageDimension = DimensionNames.Stage;

        private static Metric _measurementIngestionLatency = new Metric(
            "MeasurementIngestionLatency",
            new Dictionary<string, object>
            {
                { _nameDimension, "MeasurementIngestionLatency" },
                { _categoryDimension, Category.Latency },
                { _stageDimension, ConnectorStage.FHIRConversion },
            });

        private static Metric _measurementGroup = new Metric(
            "MeasurementGroup",
            new Dictionary<string, object>
            {
                { _nameDimension, "MeasurementGroup" },
                { _categoryDimension, Category.Traffic },
                { _stageDimension, ConnectorStage.FHIRConversion },
            });

        private static Metric _measurement = new Metric(
            "Measurement",
            new Dictionary<string, object>
            {
                { _nameDimension, "Measurement" },
                { _categoryDimension, Category.Traffic },
                { _stageDimension, ConnectorStage.FHIRConversion },
            });

        private static Metric _deviceEvent = new Metric(
            "DeviceEvent",
            new Dictionary<string, object>
            {
                { _nameDimension, "DeviceEvent" },
                { _categoryDimension, Category.Traffic },
                { _stageDimension, ConnectorStage.Normalization },
            });

        private static Metric _normalizedEvent = new Metric(
            "NormalizedEvent",
            new Dictionary<string, object>
            {
                { _nameDimension, "NormalizedEvent" },
                { _categoryDimension, Category.Traffic },
                { _stageDimension, ConnectorStage.Normalization },
            });

        private static Metric _deviceEventProcessingLatency = new Metric(
            "DeviceEventProcessingLatency",
            new Dictionary<string, object>
            {
                { _nameDimension, "DeviceEventProcessingLatency" },
                { _categoryDimension, Category.Latency },
                { _stageDimension, ConnectorStage.Normalization },
            });

        private static Metric _patientDeviceMismatch = new Metric(
            "PatientDeviceMismatchException",
            new Dictionary<string, object>
            {
                { _nameDimension, "PatientDeviceMismatchException" },
                { _categoryDimension, Category.Errors },
                { _errorTypeDimension, ErrorType.FHIRResourceError },
                { _errorSeverityDimension, ErrorSeverity.Warning },
                { _stageDimension, ConnectorStage.FHIRConversion },
            });

        private static Metric _notSupportedException = new Metric(
            "NotSupportedException",
            new Dictionary<string, object>
            {
                { _nameDimension, "NotSupportedException" },
                { _categoryDimension, Category.Errors },
                { _errorTypeDimension, ErrorType.FHIRResourceError },
                { _errorSeverityDimension, ErrorSeverity.Warning },
                { _stageDimension, ConnectorStage.FHIRConversion },
            });

        private static Metric _multipleResourceFoundException = new Metric(
           "MultipleResourceFoundException",
           new Dictionary<string, object>
           {
                { _nameDimension, "MultipleResourceFoundException" },
                { _categoryDimension, Category.Errors },
                { _errorTypeDimension, ErrorType.FHIRResourceError },
                { _errorSeverityDimension, ErrorSeverity.Warning },
                { _stageDimension, ConnectorStage.FHIRConversion },
           });

        private static Metric _templateNotFoundException = new Metric(
           "TemplateNotFoundException",
           new Dictionary<string, object>
           {
                { _nameDimension, "TemplateNotFoundException" },
                { _categoryDimension, Category.Errors },
                { _errorTypeDimension, ErrorType.GeneralError },
                { _errorSeverityDimension, ErrorSeverity.Critical },
                { _stageDimension, ConnectorStage.Unknown },
           });

        private static Metric _correlationIdNotDefinedException = new Metric(
           "CorrelationIdNotDefinedException",
           new Dictionary<string, object>
           {
                { _nameDimension, "CorrelationIdNotDefinedException" },
                { _categoryDimension, Category.Errors },
                { _errorTypeDimension, ErrorType.DeviceMessageError },
                { _errorSeverityDimension, ErrorSeverity.Critical },
                { _stageDimension, ConnectorStage.Normalization },
           });

        /// <summary>
        /// The latency between event ingestion and output to FHIR processor.
        /// </summary>
        public static Metric MeasurementIngestionLatency()
        {
            return _measurementIngestionLatency;
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
        /// An exception thrown when the patient and device references and resources do not match.
        /// </summary>
        public static Metric PatientDeviceMismatchException()
        {
            return _patientDeviceMismatch;
        }

        /// <summary>
        /// An exception thrown when the FHIR resource does not support the provided type as a value.
        /// </summary>
        public static Metric NotSupportedException()
        {
            return _notSupportedException;
        }

        /// <summary>
        /// Multiple FHIR resources were found when only one was expected.
        /// </summary>
        public static Metric MultipleResourceFoundException()
        {
            return _multipleResourceFoundException;
        }

        /// <summary>
        /// The mapping template is not defined.
        /// </summary>
        public static Metric TemplateNotFoundException()
        {
            return _templateNotFoundException;
        }

        /// <summary>
        /// An exception recorded when grouping correlation id but the correlation id is null or not found.
        /// </summary>
        public static Metric CorrelationIdNotDefinedException()
        {
            return _correlationIdNotDefinedException;
        }

        public static Metric FhirResourceNotFoundException(ResourceType resourceType)
        {
            return new Metric(
                $"{resourceType}FhirResourceNotFoundException",
                new Dictionary<string, object>
                {
                    { _nameDimension, $"{resourceType}FhirResourceNotFoundException" },
                    { _categoryDimension, Category.Errors },
                    { _errorTypeDimension, ErrorType.FHIRResourceError },
                    { _errorSeverityDimension, ErrorSeverity.Warning },
                    { _stageDimension, ConnectorStage.FHIRConversion },
                });
        }

        public static Metric ResourceIdentityNotDefinedException(ResourceType resourceType)
        {
            return new Metric(
                $"{resourceType}ResourceIdentityNotDefinedException",
                new Dictionary<string, object>
                {
                            { _nameDimension, $"{resourceType}ResourceIdentityNotDefinedException" },
                            { _categoryDimension, Category.Errors },
                            { _errorTypeDimension, ErrorType.FHIRResourceError },
                            { _errorSeverityDimension, ErrorSeverity.Warning },
                            { _stageDimension, ConnectorStage.FHIRConversion },
                });
        }

        public static Metric UnhandledException(string exceptionName, string connectorStage)
        {
            EnsureArg.IsNotNull(exceptionName);
            return new Metric(
                exceptionName,
                new Dictionary<string, object>
                {
                    { _nameDimension, exceptionName },
                    { _categoryDimension, Category.Errors },
                    { _errorTypeDimension, ErrorType.GeneralError },
                    { _errorSeverityDimension, ErrorSeverity.Critical },
                    { _stageDimension, connectorStage },
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
                    { _stageDimension, connectorStage },
                });
        }
    }
}
