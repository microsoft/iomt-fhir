// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Host;
using Microsoft.Health.Fhir.Ingest.Telemetry;
using Microsoft.Health.Fhir.Ingest.Template;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class IomtConnectorFunctions
    {
        private readonly ITelemetryLogger _logger;

        public IomtConnectorFunctions(ITelemetryLogger logger)
        {
            _logger = logger;
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
                    IomtMetrics.UnhandledException(ex.GetType().Name, ConnectorStage.FHIRConversion),
                    1);
                throw;
            }
        }

        [FunctionName("NormalizeDeviceData")]
        public async Task NormalizeDeviceData(
            [EventHubTrigger("input", Connection = "InputEventHub")] EventData[] events,
            [EventHubMeasurementCollector("output", Connection = "OutputEventHub")] IAsyncCollector<IMeasurement> output,
            [Blob("template/%Template:DeviceContent%", FileAccess.Read)] string templateDefinitions)
        {
            try
            {
                EnsureArg.IsNotNull(templateDefinitions, nameof(templateDefinitions));
                EnsureArg.IsNotNull(events, nameof(events));

                var templateContext = CollectionContentTemplateFactory.Default.Create(templateDefinitions);
                templateContext.EnsureValid();
                var template = templateContext.Template;

                _logger.LogMetric(
                    IomtMetrics.DeviceEvent(),
                    events.Length);

                IDataNormalizationService<EventData, IMeasurement> dataNormalizationService = new MeasurementEventNormalizationService(_logger, template);
                await dataNormalizationService.ProcessAsync(events, output).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogMetric(
                    IomtMetrics.UnhandledException(ex.GetType().Name, ConnectorStage.Normalization),
                    1);
                throw;
            }
        }
    }
}
