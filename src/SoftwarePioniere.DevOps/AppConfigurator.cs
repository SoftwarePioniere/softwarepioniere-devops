// using System;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Hosting;
// using SoftwarePioniere.DevOps.Infrastructure;
// using Spectre.Console.Cli;
//
// namespace SoftwarePioniere.DevOps;
//
// public static class AppConfigurator
// {
//     public static void ConfigureApp(this IHostApplicationBuilder builder)
//     {
//         var services = builder.Services;
//
//         var typeRegistrar = new Infrastructure.TypeRegistrar(services);
//
//         services.AddSingleton<ITypeRegistrar>(typeRegistrar);
//         services.AddSingleton<ITypeResolver>(p => new TypeResolver(p));
//
//         var app = new CommandApp(typeRegistrar);
//         app.RegisterCommands();
//
//         builder.Services.AddOptions<AppWorkerParams>().PostConfigure(p =>
//         {
//             p.App = app;
//             p.Args = Environment.GetCommandLineArgs();
//         });
//     }
// }