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
using Microsoft.Health.Fhir.Ingest.Validation;
using Microsoft.Health.Tools.EventDebugger.Commands;
using Microsoft.Health.Tools.EventDebugger.EventProcessor;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Fhir.Ingest.Data;

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
                BuildReplayCommand(cancellationToken),
                new ValidationCommand(),
            };

            return new CommandLineBuilder(root);
        }

        private static Command BuildReplayCommand(CancellationToken cancellationToken)
        {
            var command = new ReplayCommand();

            command.Handler = CommandHandler.Create(
                async (EventProcessorOptions eventProcessorOptions, EventConsumerOptions eventConsumerOptions, ValidationOptions validationOptions, IHost host) =>
                {
                    var serviceProvider = host.Services;
                    var processor = new DeviceEventProcessor(
                        serviceProvider.GetRequiredService<ILogger<DeviceEventProcessor>>(),
                        new EventDataJTokenConverter(),
                        serviceProvider.GetRequiredService<IIotConnectorValidator>(),
                        new LocalConversionResultWriter(eventProcessorOptions.OutputDirectory));
                    await processor.RunAsync(validationOptions, eventProcessorOptions, BuildEventHubClient(eventConsumerOptions), cancellationToken);
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
