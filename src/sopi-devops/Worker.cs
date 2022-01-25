using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SoftwarePioniere.DevOps;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly SubscriptionWorker _subscriptionWorker;
    private readonly AzureAdWorker _azureAdWorker;
    private readonly WorkerParams _parms;

    public Worker(ILogger<Worker> logger,
        IHostApplicationLifetime appLifetime,
        IOptions<WorkerParams> options,
        SubscriptionWorker subscriptionWorker,
        AzureAdWorker azureAdWorker)
    {
        _logger = logger;
        _appLifetime = appLifetime;
        _subscriptionWorker = subscriptionWorker;
        _azureAdWorker = azureAdWorker;
        _parms = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogDebug("Run Worker");
        var rootCommand = ConfigureApp();
        await rootCommand.InvokeAsync(_parms.Args);

        _appLifetime.StopApplication();
    }

    RootCommand ConfigureApp()
    {
        var rootCommand = new RootCommand();
        AppendAddCommand(rootCommand);

        {
            var cmd = new Command("subscriptions");
            rootCommand.Add(cmd);

            var loginAzCliOption = new Option<bool>("--login-az-cli",
                "Login from az cli Service Principial, not from Environment vars");

            var list = new Command("list")
            {
                loginAzCliOption
            };
            list.SetHandler((bool loginAzCli) => _subscriptionWorker.ListSubscriptions(loginAzCli), loginAzCliOption);
            cmd.Add(list);
        }

        return rootCommand;
    }

    void AppendAddCommand(Command rootCommand)
    {
        var cmd = new Command("aad");
        rootCommand.Add(cmd);

        var tempdir = @"c:\temp\aad-dev";

        var loginAzCliOption = new Option<bool>("--login-az-cli",
            "Login from az cli Service Principial, not from Environment vars");
        var dataDirOption = new Option<string>("--data-dir",
            () => tempdir,
            "The Directory which contains the data files. Defaults to Current Directory");
        var defaultPassordOption = new Option<string>("--default-password",
            () => "Password01",
            "The Default Passwort on User Creation");
        var userFilePatternOption = new Option<string>("--user-file-pattern",
            () => "benutzer*.json",
            "Pattern to read the User Files");
        var groupFilePatternOption = new Option<string>("--group-file-pattern",
            () => "gruppen*.json",
            "Pattern to read the Group Files");
        var dryRunOption = new Option<bool>("--dry-run",
            "Just dry run. No changes");

        var show = new Command("show", "Parse and list the Data Files and Load From AAD")
        {
            loginAzCliOption,
            dataDirOption,
            userFilePatternOption,
            groupFilePatternOption
        };
        cmd.Add(show);
        show.SetHandler((bool loginAzCli, string dataDir, string userFilePattern,
                string groupFilePattern)
            => _azureAdWorker.ShowAadUsersAndGroups(loginAzCli, dataDir, userFilePattern, groupFilePattern));

        var export = new Command("export", "Export existing AAD")
        {
            loginAzCliOption,
            dataDirOption,
            userFilePatternOption,
            groupFilePatternOption
        };
        cmd.Add(export);
        export.SetHandler((bool loginAzCli, string dataDir, string userFilePattern,
                string groupFilePattern) =>
            _azureAdWorker.ExportAadUsersAndGroups(loginAzCli, dataDir, userFilePattern, groupFilePattern));

        var deploy = new Command("deploy", "Deploy the AAD")
        {
            loginAzCliOption,
            dataDirOption,
            defaultPassordOption,
            userFilePatternOption,
            groupFilePatternOption,
            dryRunOption
        };
        cmd.Add(deploy);
        deploy.SetHandler((bool loginAzCli, string dataDir, string defaultPassword,
                string userFilePattern, string groupFilePattern, bool dryRun) =>
            _azureAdWorker
                .DeployAadUsersAndGroups(loginAzCli,
                    dataDir,
                    defaultPassword,
                    userFilePattern,
                    groupFilePattern,
                    dryRun));
    }
}