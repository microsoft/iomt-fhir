using System;
using System.CommandLine;
using System.IO;

namespace Microsoft.Health.Tools.EventDebugger.Commands
{
    public class ReplayCommand : BaseCommand
    {
        public ReplayCommand()
            : base("replay")
        {
            AddOption(
                new Option<int>("--totalEventsToProcess", getDefaultValue: () => 100){
                        IsRequired = false,
                        Description = "Total number of events that should be replayed",
                    });
            AddOption(
                new Option<TimeSpan>("--eventReadTimeout", getDefaultValue: () => TimeSpan.FromMinutes(5)){
                        IsRequired = false,
                        Description = "The amount of time to wait for new messages to appear. Specified as a .Net Timespan. Application will end if this timeout is reached."
                    });
            AddOption(
                new Option<string>("--connectionString"){
                        IsRequired = true,
                        Description = "The connection string to the EventHub"
                    });
            AddOption(
                new Option<string>("--consumerGroup"){
                        IsRequired = true,
                        Description = "The EventHub consumer group"
                    });
            AddOption(
                new Option<DirectoryInfo>("--outputDirectory", getDefaultValue: () => new DirectoryInfo(Directory.GetCurrentDirectory())){
                        IsRequired = false,
                        Description = "The directory to write debugging results. Defaults to the current directory"
                    });
        }
    }
}