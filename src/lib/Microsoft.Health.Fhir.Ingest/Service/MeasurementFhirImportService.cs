// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Common.Service;
using Microsoft.Health.Fhir.Ingest.Config;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Telemetry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class MeasurementFhirImportService : ParallelTaskWorker<MeasurementFhirImportOptions>
    {
        private readonly FhirImportService _fhirImportService;
        private readonly ILogger _log;

        public MeasurementFhirImportService(FhirImportService fhirImportService, MeasurementFhirImportOptions options, ILogger log)
            : base(options, options.ParallelTaskOptions.MaxConcurrency)
        {
            _fhirImportService = EnsureArg.IsNotNull(fhirImportService, nameof(fhirImportService));
            _log = EnsureArg.IsNotNull(log, nameof(log));
        }

        public async Task ProcessStreamAsync(Stream data, string templateDefinition)
        {
            EnsureArg.IsNotNull(templateDefinition, nameof(templateDefinition));
            var template = Options.TemplateFactory.Create(templateDefinition);

            var measurementGroups = await ParseAsync(data).ConfigureAwait(false);

            // Group work by device to avoid race conditions when resource creation is enabled.
            var workItems = measurementGroups.GroupBy(grp => grp.DeviceId)
                .Select(grp => new Func<Task>(
                    async () =>
                    {
                        foreach (var m in grp)
                        {
                            try
                            {
                                await _fhirImportService.ProcessAsync(template, m).ConfigureAwait(false);
                            }
                            catch (Exception ex)
                            {
                                if (!Options.ExceptionService.HandleException(ex, _log))
                                {
                                    _log.LogError(ex, ex.Message);
                                    throw;
                                }
                            }
                        }
                    }));

            await StartWorker(workItems).ConfigureAwait(false);
        }

        private async Task<IEnumerable<IMeasurementGroup>> ParseAsync(Stream data)
        {
            IList<IMeasurementGroup> measurementGroups = new List<IMeasurementGroup>();
            using (var reader = new JsonTextReader(new StreamReader(data)))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    if (reader.TokenType != JsonToken.StartObject)
                    {
                        continue;
                    }

                    var token = await JToken.ReadFromAsync(reader).ConfigureAwait(false);
                    var group = token.ToObject<MeasurementGroup>();
                    measurementGroups.Add(group);
                    _ = CalculateMetricsAsync(group.Data).ConfigureAwait(false);
                }
            }

            return measurementGroups;
        }

        private async Task CalculateMetricsAsync(IList<Measurement> measurements)
        {
            await Task.Run(() =>
            {
                DateTime nowRef = DateTime.UtcNow;

                _log.LogMetric(Metrics.MeasurementGroup, 1);
                _log.LogMetric(Metrics.Measurement, measurements.Count);

                for (int i = 0; i < measurements.Count; i++)
                {
                    var m = measurements[i];
                    if (m.IngestionTimeUtc == null)
                    {
                        continue;
                    }

                    var latency = (nowRef - m.IngestionTimeUtc.Value).TotalSeconds;
                    _log.LogMetric(Metrics.MeasurementIngestionLatency, latency);
                }
            }).ConfigureAwait(false);
        }
    }
}
