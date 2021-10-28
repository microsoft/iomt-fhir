namespace Microsoft.Health.Tools.EventDebugger
{
    public class EventConsumerOptions
    {
        public static string Category = "EventConsumer";
        public string ConnectionString {get; set;}
        public string ConsumerGroup {get; set;}
    }
}