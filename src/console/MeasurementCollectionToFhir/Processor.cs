// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Health.Events.EventConsumers;
using Microsoft.Health.Events.Model;
using Microsoft.Health.Fhir.Ingest.Host;
using Microsoft.Health.Fhir.Ingest.Service;
using Microsoft.Health.Fhir.Ingest.Telemetry;

namespace Microsoft.Health.Fhir.Ingest.Console.MeasurementCollectionToFhir
{
    public class Processor : IEventConsumer
    {
        private MeasurementFhirImportService _measurementImportService;
        private string _templateDefinition;
        private ITelemetryLogger _logger;

        public Processor(
            [Blob("template/%Template:FhirMapping%", FileAccess.Read)] string templateDefinition,
            [MeasurementFhirImport] MeasurementFhirImportService measurementImportService)
        {
            _templateDefinition = templateDefinition;
            _measurementImportService = measurementImportService;

            // todo: inject logger
            var config = new TelemetryConfiguration();
            var telemetryClient = new TelemetryClient(config);
            _logger = new IomtTelemetryLogger(telemetryClient);
        }

        public async Task<IActionResult> ConsumeAsync(IEnumerable<Event> events)
        {
            EnsureArg.IsNotNull(events);

            try
            {
                // todo: get template from blob container
                string template = File.ReadAllText("./fhirmapping.json");
                _templateDefinition = template;

                await _measurementImportService.ProcessEventsAsync(events, _templateDefinition, _logger).ConfigureAwait(false);
                return new AcceptedResult();
            }
            catch
            {
                throw;
            }
        }
    }
}
