// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Azure.WebJobs;
using Microsoft.Health.Events.EventConsumers;
using Microsoft.Health.Events.Model;
using Microsoft.Health.Fhir.Ingest.Console.Template;
using Microsoft.Health.Fhir.Ingest.Host;
using Microsoft.Health.Fhir.Ingest.Service;
using Microsoft.Health.Logger.Telemetry;

namespace Microsoft.Health.Fhir.Ingest.Console.MeasurementCollectionToFhir
{
    public class Processor : IEventConsumer
    {
        private ITemplateManager _templateManager;
        private MeasurementFhirImportService _measurementImportService;
        private string _templateDefinition;
        private ITelemetryLogger _logger;

        public Processor(
            [Blob("template/%Template:FhirMapping%", FileAccess.Read)] string templateDefinition,
            ITemplateManager templateManager,
            [MeasurementFhirImport] MeasurementFhirImportService measurementImportService,
            ITelemetryLogger logger)
        {
            _templateDefinition = templateDefinition;
            _templateManager = templateManager;
            _measurementImportService = measurementImportService;
            _logger = logger;
        }

        public async Task ConsumeAsync(IEnumerable<IEventMessage> events)
        {
            EnsureArg.IsNotNull(events);

            try
            {
                EnsureArg.IsNotNull(_templateDefinition);
                var templateContent = _templateManager.GetTemplateAsString(_templateDefinition);

                await _measurementImportService.ProcessEventsAsync(events, _templateDefinition, _logger).ConfigureAwait(false);
            }
            catch
            {
                throw;
            }
        }
    }
}
