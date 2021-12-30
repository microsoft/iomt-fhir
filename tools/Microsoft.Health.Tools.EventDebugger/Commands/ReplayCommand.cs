using System;
using System.CommandLine;
using System.IO;
using System.CommandLine.Invocation;
using System.Threading;
using Azure.Messaging.EventHubs.Consumer;
using EnsureThat;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Health.Fhir.Ingest.Validation;
using Microsoft.Health.Tools.EventDebugger.EventProcessor;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Fhir.Ingest.Data;

namespace Microsoft.Health.Tools.EventDebugger.Commands
{
    public class ReplayCommand : BaseCommand
    {
        public ReplayCommand()
            : base("replay", true)
        {
            AddOption(
                new Option<int>("--totalEventsToProcess", getDefaultValue: () => 100){
                        IsRequired = false,
                        Description = "Total number of events that should be replayed",
                    });
            AddOption(
                new Option<TimeSpan>("--eventReadTimeout", getDefaultValue: () => TimeSpan.FromMinutes(1)){
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
            AddOption(
                new Option<DateTime>("--enqueuedTime")
                {
                    IsRequired = false,
                    Description = "A specific date and time from which events will begin to be read from the EventHub. If not supplied events will be read from the beginning of the EventHub. Example: '2021-12-01T12:00:00.000-08:00'"
                });
            Handler = CommandHandler.Create(
                async (
                    EventProcessorOptions eventProcessorOptions,
                    EventConsumerOptions eventConsumerOptions,
                    ValidationOptions validationOptions,
                    IHost host,
                    CancellationToken cancellationToken) =>
                {
                    var serviceProvider = host.Services;
                    var processor = new DeviceEventProcessor(
                        serviceProvider.GetRequiredService<ILogger<DeviceEventProcessor>>(),
                        new EventDataJTokenConverter(),
                        serviceProvider.GetRequiredService<IMappingValidator>(),
                        new LocalConversionResultWriter(eventProcessorOptions.OutputDirectory));
                    await processor.RunAsync(validationOptions, eventProcessorOptions, BuildEventHubClient(eventConsumerOptions), cancellationToken);
                });
        }

        private static EventHubConsumerClient BuildEventHubClient(EventConsumerOptions eventConsumerOptions)
        {
            var connectionString = EnsureArg.IsNotNullOrWhiteSpace(eventConsumerOptions.ConnectionString, nameof(eventConsumerOptions.ConnectionString));
            var consumerGroup = EnsureArg.IsNotNullOrWhiteSpace(eventConsumerOptions.ConsumerGroup, nameof(eventConsumerOptions.ConsumerGroup));
            var eventConsumerClient = new EventHubConsumerClient(consumerGroup, connectionString);
            return eventConsumerClient;
        }
    }
}