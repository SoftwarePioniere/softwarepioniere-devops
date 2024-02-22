using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace SoftwarePioniere.DevOps;

public class AppWorker(
    IHostApplicationLifetime appLifetime,
    IOptions<AppWorkerParams> options)
    : BackgroundService
{
    private readonly AppWorkerParams _parms = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _parms.App.RunAsync(_parms.Args);
        appLifetime.StopApplication();
    }
}