using System.IO;

namespace Microsoft.Health.Tools.EventDebugger
{
    public class ValidationOptions
    {
        public FileInfo DeviceMapping {get; set;}
        public FileInfo FhirMapping {get; set;}
        public FileInfo DeviceData {get; set;}
    }
}