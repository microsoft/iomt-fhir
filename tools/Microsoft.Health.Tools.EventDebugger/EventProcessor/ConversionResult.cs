using System;
using System.Collections.Generic;
using Microsoft.Health.Fhir.Ingest.Data;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Tools.EventDebugger.EventProcessor
{
    public class ConversionResult
    {
        public JToken DeviceEvent {get; set;} 
        public IList<Measurement> Measurements {get; set;} = new List<Measurement>();
        public IList<Exception> Exceptions {get; set;} = new List<Exception>();
        public long SequenceNumber {get; set;}
    }
}