using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Tools.EventDebugger.EventProcessor;

namespace Microsoft.Health.Tools.EventDebugger
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            using var host = CreateHostBuilder(args).Build();
            using var tokenSource = GenerateCancellationToken(host.Services.GetRequiredService<IConfiguration>());
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            var deviceEventProcessor = host.Services.GetRequiredService<DeviceEventProcessor>();

            // Set up the cancellation token based on user input
            logger.LogInformation("Starting application");
            await host.StartAsync(tokenSource.Token);
            logger.LogInformation("Running Application");
            await deviceEventProcessor.RunAsync(tokenSource.Token);
            await host.StopAsync(tokenSource.Token);
            logger.LogInformation("Application Finished");
        }

        static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostingContext, serviceCollection ) => 
                {
                    Startup startup = new Startup(hostingContext.Configuration);
                    startup.ConfigureServices(serviceCollection);
                });
        }

        static CancellationTokenSource GenerateCancellationToken(IConfiguration config)
        {
            var token = new CancellationTokenSource();
            token.CancelAfter(TimeSpan.FromMinutes(10));
            return token;
        }
    }
}
