using System;
using System.IO;
namespace Microsoft.Health.Tools.EventDebugger
{
    public class EventProcessorOptions
    {
        public static string Category = "EventProcessor";
        
        public TimeSpan EventReadTimeout {get; set;} = TimeSpan.FromSeconds(60);

        public int TotalEventsToProcess {get; set;} = 10;

        public DirectoryInfo OutputDirectory {get; set;}
    }
}