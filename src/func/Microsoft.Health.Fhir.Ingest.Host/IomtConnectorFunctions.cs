// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;
using DevLab.JmesPath;
using EnsureThat;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Options;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Expressions;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Host;
using Microsoft.Health.Fhir.Ingest.Telemetry;
using Microsoft.Health.Fhir.Ingest.Template;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class IomtConnectorFunctions
    {
        private readonly ITelemetryLogger _logger;
        private readonly CollectionContentTemplateFactory _collectionContentTemplateFactory;
        private readonly IExceptionTelemetryProcessor _exceptionTelemetryProcessor;

        public IomtConnectorFunctions(ITelemetryLogger logger)
        {
            _logger = EnsureArg.IsNotNull(logger, nameof(logger));
            var expressionRegister = new AssemblyExpressionRegister(typeof(IExpressionRegister).Assembly, _logger);
            var jmesPath = new JmesPath();
            expressionRegister.RegisterExpressions(jmesPath.FunctionRepository);
            _collectionContentTemplateFactory = new CollectionContentTemplateFactory(
                new JsonPathContentTemplateFactory(),
                new IotJsonPathContentTemplateFactory(),
                new IotCentralJsonPathContentTemplateFactory(),
                new CalculatedFunctionContentTemplateFactory(
                    new TemplateExpressionEvaluatorFactory(jmesPath), _logger));
            _exceptionTelemetryProcessor = new NormalizationExceptionTelemetryProcessor();
        }

        [FunctionName("MeasurementCollectionToFhir")]
        public async Task<IActionResult> MeasurementCollectionToFhir(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [Blob("template/%Template:FhirMapping%", FileAccess.Read)] string templateDefinition,
            [MeasurementFhirImport] MeasurementFhirImportService measurementImportService)
        {
            EnsureArg.IsNotNull(measurementImportService, nameof(measurementImportService));
            EnsureArg.IsNotNull(req, nameof(req));

            try
            {
                await measurementImportService.ProcessStreamAsync(req.Body, templateDefinition, _logger).ConfigureAwait(false);
                return new AcceptedResult();
            }
            catch (Exception ex)
            {
                _logger.LogMetric(
                    IomtMetrics.UnhandledException(ex.GetType().Name, ConnectorOperation.FHIRConversion),
                    1);
                throw;
            }
        }

        [FunctionName("NormalizeDeviceData")]
        public async Task NormalizeDeviceData(
            [EventHubTrigger("input", Connection = "InputEventHub")] EventData[] events,
            [EventHubMeasurementCollector("output", Connection = "OutputEventHub")] IAsyncCollector<IMeasurement> output,
            [Blob("template/%Template:DeviceContent%", FileAccess.Read)] string templateDefinitions,
            [DeviceDataNormalization] IOptions<NormalizationServiceOptions> normalizationSettings)
        {
            try
            {
                EnsureArg.IsNotNull(templateDefinitions, nameof(templateDefinitions));
                EnsureArg.IsNotNull(events, nameof(events));
                EnsureArg.IsNotNull(normalizationSettings, nameof(normalizationSettings));

                var templateContext = _collectionContentTemplateFactory.Create(templateDefinitions);
                templateContext.EnsureValid();
                var template = templateContext.Template;

                _logger.LogMetric(
                    IomtMetrics.DeviceEvent(),
                    events.Length);

                IDataNormalizationService<EventData, IMeasurement> dataNormalizationService = new MeasurementEventNormalizationService(_logger, template, _exceptionTelemetryProcessor);
                await dataNormalizationService.ProcessAsync(events, output).ConfigureAwait(false);

                if (normalizationSettings.Value.LogDeviceIngressSizeBytes)
                {
                    IEventProcessingMeter meter = new EventProcessingMeter();
                    var eventStats = await meter.CalculateEventStats(events);
                    _logger.LogMetric(
                        IomtMetrics.DeviceIngressSizeBytes(),
                        eventStats.TotalEventsProcessedBytes);
                }
            }
            catch (Exception ex)
            {
                _logger.LogMetric(
                    IomtMetrics.UnhandledException(ex.GetType().Name, ConnectorOperation.Normalization),
                    1);
                throw;
            }
        }
    }
}