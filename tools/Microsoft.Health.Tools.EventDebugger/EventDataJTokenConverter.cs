using System.Text;
using EnsureThat;
using Azure.Messaging.EventHubs;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Ingest.Data
{
    public class EventDataJTokenConverter : IConverter<EventData, JToken>
    {
        public JToken Convert(EventData input)
        {
            EnsureArg.IsNotNull(input, nameof(input));

            var body = JToken.Parse(Encoding.UTF8.GetString(input.EventBody));
            var data = new { Body = body, input.Properties, input.SystemProperties };
            var token = JToken.FromObject(data);
            return token;
        }
    }
}
