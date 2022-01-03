using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Health.Fhir.Ingest.Validation;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

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
                    var deviceData = validationOptions.DeviceData != null ? JToken.Parse(File.ReadAllText(validationOptions.DeviceData.FullName)) : null;
                    var fhirMapping = validationOptions.FhirMapping != null ? File.ReadAllText(validationOptions.FhirMapping.FullName) : null;;
                    var deviceMapping = validationOptions.DeviceMapping !=null ? File.ReadAllText(validationOptions.DeviceMapping.FullName) : null;

                    var serializerSettings = new Newtonsoft.Json.JsonSerializer()
                    {
                        NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                        ContractResolver = SkipEmptyCollectionsContractResolver.Instance,
                    };
                    serializerSettings.Converters.Add(new StringEnumConverter());

                    if (string.IsNullOrWhiteSpace(deviceMapping) && string.IsNullOrWhiteSpace(fhirMapping))
                    {
                        Console.WriteLine("Validation cannot be performed: No device or fhir mapping were supplied.");
                    }
                    else
                    {
                        var serviceProvider = host.Services;
                        var validator = serviceProvider.GetRequiredService<IMappingValidator>();
                        var result = validator.PerformValidation(deviceData, deviceMapping, fhirMapping);
                        Console.WriteLine(JToken.FromObject(result, serializerSettings).ToString());
                    }
                });
        }
    }
}