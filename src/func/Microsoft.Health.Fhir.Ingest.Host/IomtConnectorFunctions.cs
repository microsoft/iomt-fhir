// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
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
        public async Task MeasurementCollectionToFhir(
            [Common.EventHubs.EventHubTrigger("fhir", Connection = "OutputEventHub")] EventData[] events,
            [Blob("template/%Template:FhirMapping%", FileAccess.Read)] string templateDefinition,
            [MeasurementFhirImport] MeasurementFhirImportService measurementImportService)
        {
            EnsureArg.IsNotNull(measurementImportService, nameof(measurementImportService));
            EnsureArg.IsNotNull(events, nameof(events));

            try
            {
                await measurementImportService.ProcessEventsAync(events, templateDefinition, _logger).ConfigureAwait(false);
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
            [Azure.WebJobs.EventHubTrigger("input", Connection = "InputEventHub")] EventData[] events,
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
                    IomtMetrics.UnhandledException(ex.GetType().Name, ConnectorOperation.Normalization),
                    1);
                throw;
            }
        }
    }
}
