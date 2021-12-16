using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Health.Tools.EventDebugger.Commands;
namespace Microsoft.Health.Tools.EventDebugger
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await BuildCommandLine()
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

        private static CommandLineBuilder BuildCommandLine()
        {
            var root = new RootCommand("debugger"){
                new ReplayCommand(),
                new ValidationCommand(),
            };

            return new CommandLineBuilder(root);
        }
    }
}
