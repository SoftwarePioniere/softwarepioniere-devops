using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Spectre.Console.Cli;


namespace SoftwarePioniere.DevOps
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            var serilogger = CreateSerilogger();

            try
            {
                var host = Host.CreateDefaultBuilder()
                    .ConfigureLogging(builder =>
                        {
                            builder.ClearProviders();
                            builder.AddSerilog(serilogger);
                        }
                    )
                    .ConfigureServices(services =>
                    {
                        services
                            .AddSingleton<SubscriptionWorker>()
                            .AddSingleton<AzureAdWorker>()
                            ;

                        services.AddOptions<WorkerParams>()
                            .PostConfigure(p => { p.Args = args; });

                        services.AddSingleton<ITypeRegistrar>(new TypeRegistrar(services));

                        services.AddHostedService<Worker>();
                    })
                    .Build();

                await host.RunAsync();
            }
            catch (Exception e)
            {
                serilogger.Error(e, "Unhandled Error");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        static Serilog.ILogger CreateSerilogger()
        {
            var config = new LoggerConfiguration()
                    .Enrich.WithProperty("AppId", "dotnet-sopi-devops")
                ;

            const string template =
                "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:l}{Exception}{NewLine}";

            config.WriteTo.LiterateConsole(
                outputTemplate: template);

            Log.Logger = config.CreateLogger();
            return Log.Logger;
        }
    }
}