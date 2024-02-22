using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Spectre.Console.Cli;

namespace SoftwarePioniere.DevOps;

public class Worker(
    IHostApplicationLifetime appLifetime,
    IOptions<WorkerParams> options,
    ITypeRegistrar typeRegistrar)
    : BackgroundService
{
    private readonly WorkerParams _parms = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var app = new CommandApp(typeRegistrar);
        app.RegisterCommands();
        await app.RunAsync(_parms.Args);
        appLifetime.StopApplication();
    }
}