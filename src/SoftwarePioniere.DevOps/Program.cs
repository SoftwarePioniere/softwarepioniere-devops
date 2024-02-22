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


// using System;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.Logging;
// using Serilog;
// using SoftwarePioniere.DevOps;
//
// var serilogger = CreateSerilogger();
//
// var builder = Host.CreateApplicationBuilder(args);
//
// builder.Logging.ClearProviders();
// builder.Logging.AddSerilog(serilogger);
// builder.Services.AddHostedService<AppWorker>();
// builder.ConfigureApp();
//
// try
// {
//     var host = builder.Build();
//     host.Run();
// }
// catch (Exception e)
// {
//     serilogger.Error(e, "Unhandled Error");
// }
// finally
// {
//     Log.CloseAndFlush();
// }
//
//
//
// static Serilog.ILogger CreateSerilogger()
// {
//     var config = new LoggerConfiguration()
//             .Enrich.WithProperty("AppId", "dotnet-sopi-devops")
//         ;
//
//     const string Template =
//         "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:l}{Exception}{NewLine}";
//
//     config.WriteTo.LiterateConsole(
//         outputTemplate: Template);
//
//     Log.Logger = config.CreateLogger();
//     return Log.Logger;
// }