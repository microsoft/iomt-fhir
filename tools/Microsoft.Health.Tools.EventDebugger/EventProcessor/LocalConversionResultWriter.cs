using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Extensions.Configuration;
using Microsoft.Health.Fhir.Ingest.Validation;
using Microsoft.Health.Tools.EventDebugger;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Tools.EventDebugger.EventProcessor
{
    public class LocalConversionResultWriter : IConversionResultWriter
    {
        private readonly DirectoryInfo _outputDirectory;
        private readonly DateTime _timeOfExecution;
        private readonly JsonSerializer _jsonSerializer;
        public LocalConversionResultWriter(
            DirectoryInfo runDirectory)
        {
            EnsureArg.IsNotNull(runDirectory, nameof(runDirectory));
            runDirectory.Create();
            _timeOfExecution = DateTime.Now;
            _outputDirectory = runDirectory.CreateSubdirectory($"run_{_timeOfExecution.ToString("s")}");
            _jsonSerializer = new JsonSerializer()
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = SkipEmptyCollectionsContractResolver.Instance,
            };
        }

        public async Task StoreConversionResult(ValidationResult conversionResult, CancellationToken cancellationToken = default)
        {
            // Create a datetime stamped folder if needed
            var storageFolder = CreateStorageFolder(conversionResult);
            // Store a new JToken which holds the DeviceEvent, Measurements and Exceptions. Store in a file with the Sequence Id as the name
            var data = new { DeviceEvent = conversionResult.DeviceEvent, Measurements = conversionResult.Measurements, Exceptions = conversionResult.Exceptions };
            await File.WriteAllTextAsync( 
                Path.Join(storageFolder.ToString(), $"{conversionResult.SequenceNumber}.json"),
                JToken.FromObject(data, _jsonSerializer).ToString(),
                cancellationToken);
        }

        private DirectoryInfo CreateStorageFolder(ValidationResult result)
        {
            if ((result.Exceptions.Count + result.Warnings.Count) == 0)
            {
                return _outputDirectory.CreateSubdirectory("success");
            }
            else
            {
                return _outputDirectory.CreateSubdirectory("withErrors");
            }
        }
    }
}