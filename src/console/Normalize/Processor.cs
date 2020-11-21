using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Health.Events.EventConsumers;
using Microsoft.Health.Events.Model;
using Microsoft.Health.Fhir.Ingest.Config;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Service;
using Microsoft.Health.Fhir.Ingest.Telemetry;
using Microsoft.Health.Fhir.Ingest.Template;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.Azure.EventHubs.EventData;

namespace Microsoft.Health.Fhir.Ingest.Console.Normalize
{
    public class Processor : IEventConsumer
    {
        private string _templateDefinitions;
        private ITelemetryLogger _logger;
        private IConfiguration _env;
        private IOptions<EventHubMeasurementCollectorOptions> _options;

        public Processor(
            [Blob("template/%Template:DeviceContent%", FileAccess.Read)] string templateDefinitions,
            IConfiguration configuration,
            IOptions<EventHubMeasurementCollectorOptions> options)
        {
            _templateDefinitions = templateDefinitions;

            var config = new TelemetryConfiguration();
            var telemetryClient = new TelemetryClient(config);
            _logger = new IomtTelemetryLogger(telemetryClient);
            _env = configuration;
            _options = options;
        }

        public async Task<IActionResult> ConsumeAsync(IEnumerable<Event> events)
        {
            // todo: get template from blob container
            string readText = File.ReadAllText("./devicecontent.json");
            _templateDefinitions = readText;

            var templateContext = CollectionContentTemplateFactory.Default.Create(_templateDefinitions);
            templateContext.EnsureValid();
            var template = templateContext.Template;

            _logger.LogMetric(
                IomtMetrics.DeviceEvent(),
                    events.Count());

            IEnumerable<EventData> eventHubEvents = events
                .Select(x => {
                    var eventData = new EventData(x.Body.ToArray());
                    eventData.SystemProperties = new SystemPropertiesCollection(
                        x.SequenceNumber,
                        x.EnqueuedTime.UtcDateTime,
                        x.Offset.ToString(),
                        x.PartitionId);

                    foreach (KeyValuePair<string, object> entry in x.SystemProperties)
                    {
                        eventData.SystemProperties.TryAdd(entry.Key, entry.Value);
                    }
                    
                    return eventData;
                })
                .ToList();

            var dataNormalizationService = new MeasurementEventNormalizationService(_logger, template);

            var connectionString = _env.GetSection("OutputEventHub").Value;
            var eventHubName = connectionString.Substring(connectionString.LastIndexOf('=') + 1);
            
            var collector = CreateCollector(eventHubName, connectionString, _options);

            await dataNormalizationService.ProcessAsync(eventHubEvents, collector).ConfigureAwait(false);

            return new AcceptedResult();
        }

        private IAsyncCollector<IMeasurement> CreateCollector(string eventHubName, string connectionString, IOptions<EventHubMeasurementCollectorOptions> options)
        {
            var client = options.Value.GetEventHubClient(eventHubName, connectionString);
            return new MeasurementToEventAsyncCollector(new EventHubService(client));
        }
    }
}
