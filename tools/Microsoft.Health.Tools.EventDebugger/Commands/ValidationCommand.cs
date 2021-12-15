using System;
using System.IO;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Health.Fhir.Ingest.Validation;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Tools.EventDebugger.Commands
{
    public class ValidationCommand : BaseCommand
    {
        public ValidationCommand()
            : base("validate")
        {
            AddOption(
                new Option<FileInfo>("--deviceData"){
                        IsRequired = false,
                        Description = "The path to the file containing sample device data",
                    });
            Handler = CommandHandler.Create(
                (ValidationOptions validationOptions, IHost host) =>
                {
                    var deviceData = validationOptions.DeviceData != null? JToken.Parse(File.ReadAllText(validationOptions.DeviceData.FullName)) : null;
                    var fhirMapping = validationOptions.FhirMapping != null? File.ReadAllText(validationOptions.FhirMapping.FullName) : null;;
                    var deviceMapping = File.ReadAllText(validationOptions.DeviceMapping.FullName);
                    var serializerSettings = new Newtonsoft.Json.JsonSerializer()
                    {
                        NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                        ContractResolver = SkipEmptyCollectionsContractResolver.Instance,
                    };

                    var serviceProvider = host.Services;
                    var validator = serviceProvider.GetRequiredService<IIotConnectorValidator>();
                    var result = validator.PerformValidation(deviceData, deviceMapping, fhirMapping);
                    Console.WriteLine(JToken.FromObject(result, serializerSettings).ToString());
                });
        }
    }
}