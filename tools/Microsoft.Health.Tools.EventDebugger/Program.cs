using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs.Consumer;
using EnsureThat;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Health.Tools.EventDebugger.EventProcessor;

namespace Microsoft.Health.Tools.EventDebugger
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var token = new CancellationTokenSource();
            token.CancelAfter(TimeSpan.FromMinutes(10));

            await BuildCommandLine(token.Token)
                .UseHost(_ => Host.CreateDefaultBuilder(args), builder => ConfigureServices(builder))
                .UseDefaults()
                .Build()
                .InvokeAsync(args);
        }

        static IHostBuilder ConfigureServices(IHostBuilder builder)
        {
            return builder
                .ConfigureServices((hostingContext, serviceCollection ) => 
                {
                    Startup startup = new Startup(hostingContext.Configuration);
                    startup.ConfigureServices(serviceCollection);
                });
        }

        private static CommandLineBuilder BuildCommandLine(CancellationToken cancellationToken)
        {
            var root = new RootCommand("debugger"){
                BuildReplayCommand(cancellationToken)
            };

            return new CommandLineBuilder(root);
        }

        private static Command BuildReplayCommand(CancellationToken cancellationToken)
        {
            var command = new Command("replay"){
                    new Option<int>("--TotalEventsToProcess", getDefaultValue: () => 100){
                        IsRequired = false,
                        Description = "Total number of events that should be replayed",
                    },
                    new Option<TimeSpan>("--EventReadTimeout", getDefaultValue: () => TimeSpan.FromMinutes(5)){
                        IsRequired = false,
                        Description = "The amount of time to wait for new messages to appear. Specified as a .Net Timespan. Application will end if this timeout is reached."
                    },
                    new Option<string>("--ConnectionString"){
                        IsRequired = true,
                        Description = "The connection string to the EventHub"
                    },
                    new Option<string>("--ConsumerGroup"){
                        IsRequired = true,
                        Description = "The EventHub consumer group"
                    }
                };

            command.Handler = CommandHandler.Create(
                async (EventProcessorOptions eventProcessorOptions, EventConsumerOptions eventConsumerOptions, IHost host) =>
                {
                    var serviceProvider = host.Services;
                    var deviceEventProcessor = serviceProvider.GetRequiredService<DeviceEventProcessor>();
                    await deviceEventProcessor.RunAsync(eventProcessorOptions, BuildEventHubClient(eventConsumerOptions), cancellationToken);
                });
            return command;
        }

        static EventHubConsumerClient BuildEventHubClient(EventConsumerOptions eventConsumerOptions)
        {
            var connectionString = EnsureArg.IsNotNullOrWhiteSpace(eventConsumerOptions?.ConnectionString, nameof(eventConsumerOptions.ConnectionString));
            var consumerGroup = EnsureArg.IsNotNullOrWhiteSpace(eventConsumerOptions.ConsumerGroup, nameof(eventConsumerOptions.ConsumerGroup));
            var eventConsumerClient = new EventHubConsumerClient(consumerGroup, connectionString);
            return eventConsumerClient;
        }


    }
}
