using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Health.Fhir.Ingest.Validation.Extensions;
using Microsoft.Health.Fhir.Ingest.Validation.Models;
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

        public async Task StoreConversionResult(DebugResult conversionResult, CancellationToken cancellationToken = default)
        {
            var validationResult = conversionResult.ValidationResult;
            // Create a datetime stamped folder if needed
            var storageFolder = CreateStorageFolder(validationResult);
            // Store a new JToken which holds the DeviceEvent, Measurements and Exceptions. Store in a file with the Sequence Id as the name
            // The Debugger stores a single DeviceEvent per ValidationResult
            var deviceData = validationResult.DeviceResults.First();

            var data = new {
                TemplateDetails = validationResult.TemplateResult,
                DeviceDetails = new
                    {
                        DeviceEvent = deviceData.DeviceEvent,
                        Exceptions = deviceData.GetErrors(ErrorLevel.ERROR),
                        Warnings = deviceData.GetErrors(ErrorLevel.WARN),
                        Measurements = deviceData.Measurements,
                        Observations = deviceData.Observations,
                    },
                };
            await File.WriteAllTextAsync( 
                Path.Join(storageFolder.ToString(), $"{conversionResult.SequenceNumber}.json"),
                JToken.FromObject(data, _jsonSerializer).ToString(),
                cancellationToken);
        }

        private DirectoryInfo CreateStorageFolder(ValidationResult result)
        {
            if (result.AnyException(ErrorLevel.ERROR) || result.AnyException(ErrorLevel.WARN))
            {
                return _outputDirectory.CreateSubdirectory("withErrors");
            }
            else
            {
                return _outputDirectory.CreateSubdirectory("success");
            }
        }
    }
}