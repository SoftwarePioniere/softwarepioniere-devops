using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SoftwarePioniere.DevOps.Commands;
using SoftwarePioniere.DevOps.Infrastructure;
using Spectre.Console.Cli;

namespace SoftwarePioniere.DevOps;

public static class Program
{
    public static Task<int> Main(string[] args)
    {
        var services = new ServiceCollection()
            .AddLogging(configure =>
            {
                const string Template = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:l}{Exception}{NewLine}";
                configure.AddSerilog(new LoggerConfiguration()
                    // log level will be dynamically be controlled by our log interceptor upon running
                    .MinimumLevel.ControlledBy(LogInterceptor.LogLevel)
                    .WriteTo.LiterateConsole(outputTemplate: Template)
                    .CreateLogger());
            }).RegisterServices();

        var registrar = new TypeRegistrar(services);
        var app = new CommandApp<HelloCommand>(registrar);
        app.Configure(config =>
        {
            config.SetInterceptor(new LogInterceptor()); // add the interceptor
            config.RegisterCommands();
        });

        return app.RunAsync(args);
    }
}