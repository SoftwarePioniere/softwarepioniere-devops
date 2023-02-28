using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Spectre.Console.Cli;

namespace SoftwarePioniere.DevOps;

public class Worker : BackgroundService
{
    
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly ITypeRegistrar _typeRegistrar;
    private readonly WorkerParams _parms;

    public Worker(
        IHostApplicationLifetime appLifetime,
        IOptions<WorkerParams> options,
        ITypeRegistrar typeRegistrar)
    {
     
        _appLifetime = appLifetime;
        _typeRegistrar = typeRegistrar;
        _parms = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var app = new CommandApp(_typeRegistrar);

        app.Configure(config =>
        {
            config.AddBranch("aad",
                aad =>
                {
                    aad.AddCommand<ShowSubscriptionsCommand>("subscriptions")
                        .WithDescription("List all Azure Subscriptions");
                    aad.AddCommand<ShowAadUsersAndGroupsCommand>("show")
                        .WithDescription("Parse and list the Data Files and Load From AAD");
                    aad.AddCommand<ExportAadUsersAndGroupsCommand>("export")
                        .WithDescription("Export existing AAD");
                    aad.AddCommand<DeployAadUsersAndGroupsCommand>("deploy")
                        .WithDescription("Deploy the AAD");
                });
        });

        await app.RunAsync(_parms.Args);

        _appLifetime.StopApplication();
    }

}