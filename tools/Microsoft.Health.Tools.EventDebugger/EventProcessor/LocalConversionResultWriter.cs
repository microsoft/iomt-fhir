using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Extensions.Configuration;
using Microsoft.Health.Tools.EventDebugger.Extensions;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Tools.EventDebugger.EventProcessor
{
    public class LocalConversionResultWriter : IConversionResultWriter
    {
        private readonly DirectoryInfo _outputDirectory;
        private readonly DateTime _timeOfExecution;
        public LocalConversionResultWriter(IConfiguration configuration)
        {
            EnsureArg.IsNotNull(configuration, nameof(configuration));
            var outputDirectory = configuration.GetArgument("OutputDirectory", true);
            var runDirectory = Directory.CreateDirectory(outputDirectory);
            _timeOfExecution = DateTime.Now;
            _outputDirectory = runDirectory.CreateSubdirectory($"run_{_timeOfExecution.ToString("s")}");
        }

        public async Task StoreConversionResult(ConversionResult conversionResult, CancellationToken cancellationToken = default)
        {
            // Create a datetime stamped folder if needed
            var storageFolder = CreateStorageFolder(conversionResult);
            var errors = conversionResult.Exceptions.Select(e => e.Message);
            // Store a new JToken which holds the DeviceEvent, Measurements and Exceptions. Store in a file with the Sequence Id as the name
            var data = new { DeviceEvent = conversionResult.DeviceEvent, Measurements = conversionResult.Measurements, Exceptions = errors };
            await File.WriteAllTextAsync( 
                Path.Join(storageFolder.ToString(), $"{conversionResult.SequenceNumber}.json"),
                JToken.FromObject(data).ToString(),
                cancellationToken);
        }

        private DirectoryInfo CreateStorageFolder(ConversionResult result)
        {
            if (result.Exceptions.Count == 0)
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