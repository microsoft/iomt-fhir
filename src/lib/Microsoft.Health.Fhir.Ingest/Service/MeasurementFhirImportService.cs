﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Health.Common.Service;
using Microsoft.Health.Events.Errors;
using Microsoft.Health.Events.Model;
using Microsoft.Health.Fhir.Ingest.Config;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Telemetry;
using Microsoft.Health.Fhir.Ingest.Template;
using Microsoft.Health.Logging.Telemetry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class MeasurementFhirImportService : ParallelTaskWorker<MeasurementFhirImportOptions>
    {
        private readonly FhirImportService _fhirImportService;
        private readonly IExceptionTelemetryProcessor _exceptionTelemetryProcessor;

        public MeasurementFhirImportService(FhirImportService fhirImportService, MeasurementFhirImportOptions options, IExceptionTelemetryProcessor exceptionTelemetryProcessor)
            : base(options, options?.ParallelTaskOptions?.MaxConcurrency ?? 1)
        {
            _fhirImportService = EnsureArg.IsNotNull(fhirImportService, nameof(fhirImportService));
            _exceptionTelemetryProcessor = exceptionTelemetryProcessor;
        }

        public async Task ProcessStreamAsync(Stream data, string templateDefinition, ITelemetryLogger log)
        {
            var template = BuildTemplate(templateDefinition, log);
            var measurementGroups = await ParseAsync(data, log).ConfigureAwait(false);

            await ProcessMeasurementGroups(measurementGroups, template, log).ConfigureAwait(false);
        }

        public async Task ProcessEventsAsync(IEnumerable<IEventMessage> events, string templateDefinition, ITelemetryLogger log)
        {
            // Step 1: Before processing events, validate the template
            ILookupTemplate<IFhirTemplate> template = null;

            try
            {
                template = BuildTemplate(templateDefinition, log);
            }
            catch (Exception ex)
            {
                ex.AddEventContext(events);

                if (!_exceptionTelemetryProcessor.HandleException(ex, log))
                {
                    throw;
                }
                else
                {
                    return; // don't continue to next step since the template is not valid
                }
            }

            // Step 2: Transform the events into Measurements and group into Measurement Groups
            Tuple<IEnumerable<IMeasurementGroup>, Dictionary<IMeasurement, IEventMessage>> parseEventData;
            IEnumerable<IMeasurementGroup> measurementGroups = null;
            Dictionary<IMeasurement, IEventMessage> measurementToEventMapping = null;

            try
            {
                parseEventData = ParseEventData(events, log);
                measurementGroups = parseEventData.Item1;
                measurementToEventMapping = parseEventData.Item2;
            }
            catch (Exception ex)
            {
                if (!_exceptionTelemetryProcessor.HandleException(ex, log))
                {
                    throw;
                }
                else
                {
                    Console.WriteLine(events.Count());

                    // remove the event(s) from the batch
                    var eventsWithExceptions = ex.GetRelatedLegacyEvents();
                    events = events.Where(e => !eventsWithExceptions.Contains(e)).ToList();

                    Console.WriteLine(events.Count());
                }
            }

            // Step 3: Transform the Measurement Groups into Observations and write to the FHIR Server.
            //         Exceptions are currently handled in ProcessMeasurementGroups
            await ProcessMeasurementGroups(measurementGroups, template, log, measurementToEventMapping).ConfigureAwait(false);
        }

        private ILookupTemplate<IFhirTemplate> BuildTemplate(string templateDefinition, ITelemetryLogger log)
        {
            EnsureArg.IsNotNull(templateDefinition, nameof(templateDefinition));
            EnsureArg.IsNotNull(log, nameof(log));

            var templateContext = Options.TemplateFactory.Create(templateDefinition);
            templateContext.EnsureValid();

            return templateContext.Template;
        }

        private async Task ProcessMeasurementGroups(IEnumerable<IMeasurementGroup> measurementGroups, ILookupTemplate<IFhirTemplate> template, ITelemetryLogger log, Dictionary<IMeasurement, IEventMessage> eventLookup = null)
        {
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
                                if (eventLookup != null)
                                {
                                    var events = new List<IEventMessage>();

                                    foreach (var measurement in m.Data)
                                    {
                                        var eventId = eventLookup[measurement];
                                        events.Add(eventId);
                                    }

                                    ex.AddEventContext(events);
                                }

                                if (!_exceptionTelemetryProcessor.HandleException(ex, log))
                                {
                                    throw;
                                }
                            }
                        }
                    }));

            await StartWorker(workItems).ConfigureAwait(false);
        }

        private static async Task<IEnumerable<IMeasurementGroup>> ParseAsync(Stream data, ITelemetryLogger log)
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
                    _ = CalculateMetricsAsync(group.Data, log).ConfigureAwait(false);
                }
            }

            return measurementGroups;
        }

        private static Tuple<IEnumerable<IMeasurementGroup>, Dictionary<IMeasurement, IEventMessage>> ParseEventData(IEnumerable<IEventMessage> data, ITelemetryLogger logger)
        {
            var partitionId = data.FirstOrDefault()?.PartitionId;

            var dictionary = new Dictionary<IMeasurement, IEventMessage>();

            IEnumerable<IMeasurementGroup> measurementGroups = data.Select(e =>
            {
                try
                {
                    var measurement = JsonConvert.DeserializeObject<Measurement>(System.Text.Encoding.Default.GetString(e.Body.ToArray()));
                    dictionary.Add(measurement, e);
                    return measurement;
                }
                catch (Exception ex)
                {
                    throw new MeasurementProcessingException(ex, e);
                }
            })
            .GroupBy(m => $"{m.DeviceId}-{m.Type}-{m.PatientId}-{m.EncounterId}-{m.CorrelationId}")
            .Select(g =>
            {
                IList<Measurement> measurements = g.ToList();
                _ = CalculateMetricsAsync(measurements, logger, partitionId).ConfigureAwait(false);
                return new MeasurementGroup
                {
                    Data = measurements,
                    MeasureType = measurements[0].Type,
                    CorrelationId = measurements[0].CorrelationId,
                    DeviceId = measurements[0].DeviceId,
                    EncounterId = measurements[0].EncounterId,
                    PatientId = measurements[0].PatientId,
                };
            })
            .ToArray();

            return Tuple.Create(measurementGroups, dictionary);
        }

        private static async Task CalculateMetricsAsync(IList<Measurement> measurements, ITelemetryLogger log, string partitionId = null)
        {
            await Task.Run(() =>
            {
                DateTime nowRef = DateTime.UtcNow;

                log.LogMetric(
                    IomtMetrics.MeasurementGroup(partitionId),
                    1);

                log.LogMetric(
                    IomtMetrics.Measurement(partitionId),
                    measurements.Count);

                for (int i = 0; i < measurements.Count; i++)
                {
                    var m = measurements[i];
                    if (m.IngestionTimeUtc == null)
                    {
                        continue;
                    }

                    log.LogMetric(
                        IomtMetrics.MeasurementIngestionLatency(partitionId),
                        (nowRef - m.IngestionTimeUtc.Value).TotalSeconds);

                    log.LogMetric(
                        IomtMetrics.MeasurementIngestionLatencyMs(partitionId),
                        (nowRef - m.IngestionTimeUtc.Value).TotalMilliseconds);
                }
            }).ConfigureAwait(false);
        }
    }
}