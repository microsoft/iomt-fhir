using System.CommandLine;
using System.IO;

namespace Microsoft.Health.Tools.EventDebugger.Commands
{
    public class BaseCommand : Command
    {
        public BaseCommand(
            string commandName,
            bool isDeviceMappingRequired = false)
            : base(commandName)
        {
            AddOption(
                new Option<FileInfo>("--deviceMapping"){
                        IsRequired = isDeviceMappingRequired,
                        Description = "The path to the device mapping template file",
                    });
            AddOption(
                new Option<FileInfo>("--fhirMapping"){
                        Description = "The path to the fhir mapping template file",
                    });
        }
    }
}