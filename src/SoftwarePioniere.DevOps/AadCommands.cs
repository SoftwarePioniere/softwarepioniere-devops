using System.ComponentModel;
using System.Threading.Tasks;
using Spectre.Console.Cli;

// ReSharper disable ClassNeverInstantiated.Global

namespace SoftwarePioniere.DevOps;

public class MyCommandSettings : CommandSettings
{
    // [CommandOption("--url")]
    // [DefaultValue("localhost")]
    // public string Url { get; set; }
    //
    // [CommandOption("--port")]
    // [DefaultValue(2113)]
    // public int Port { get; set; }

    [CommandOption("--login-az-cli")]
    [DefaultValue(false)]
    public bool LoginAzCli { get; set; }
}

public abstract class MyCommandBase<T> : AsyncCommand<T> where T : CommandSettings
{
    // protected EventStoreSetupLogic CreateLogic()
    // {
    //     var logger = LoggerFactory.Create(builder => builder.AddSimpleConsole(options =>
    //         {
    //             options.IncludeScopes = true;
    //             options.SingleLine = true;
    //             options.TimestampFormat = "HH:mm:ss ";
    //         }))
    //         .CreateLogger(GetType());
    //     var logic = new EventStoreSetupLogic(logger);
    //     return logic;
    // }
}

public class ShowSubscriptionsCommand : MyCommandBase<MyCommandSettings>
{
    private readonly SubscriptionWorker _worker;

    public ShowSubscriptionsCommand(SubscriptionWorker worker)
    {
        _worker = worker;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, MyCommandSettings settings)
    {
        await _worker.ListSubscriptions(settings.LoginAzCli);
        return 0;
    }
}

public class ShowAadUsersAndGroupsCommand : MyCommandBase<ShowAadUsersAndGroupsCommand.Settings>
{
    private readonly AzureAdWorker _worker;

    public class Settings : MyCommandSettings
    {
        [CommandOption("--data-dir")]
        public string DataDir { get; set; }

        [CommandOption("--user-file-pattern")]
        [DefaultValue("benutzer*.json")]
        public string UserFilePattern { get; set; }

        [CommandOption("--group-file-pattern")]
        [DefaultValue("gruppen*.json")]
        public string GroupFilePattern { get; set; }
    }

    public ShowAadUsersAndGroupsCommand(AzureAdWorker worker)
    {
        _worker = worker;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, ShowAadUsersAndGroupsCommand.Settings settings)
    {
        await _worker.ShowAadUsersAndGroups(settings.LoginAzCli,
            settings.DataDir,
            settings.UserFilePattern,
            settings.GroupFilePattern);
        return 0;
    }
}




public class ExportAadUsersAndGroupsCommand : MyCommandBase<ExportAadUsersAndGroupsCommand.Settings>
{
    private readonly AzureAdWorker _worker;

    public class Settings : MyCommandSettings
    {
        [CommandOption("--data-dir")]
        public string DataDir { get; set; }

        [CommandOption("--user-file-pattern")]
        [DefaultValue("benutzer*.json")]
        public string UserFilePattern { get; set; }

        [CommandOption("--group-file-pattern")]
        [DefaultValue("gruppen*.json")]
        public string GroupFilePattern { get; set; }
    }

    public ExportAadUsersAndGroupsCommand(AzureAdWorker worker)
    {
        _worker = worker;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, ExportAadUsersAndGroupsCommand.Settings settings)
    {
        await _worker.ExportAadUsersAndGroups(settings.LoginAzCli,
            settings.DataDir,
            settings.UserFilePattern,
            settings.GroupFilePattern);
        return 0;
    }
}





public class DeployAadUsersAndGroupsCommand : MyCommandBase<DeployAadUsersAndGroupsCommand.Settings>
{
    private readonly AzureAdWorker _worker;

    public class Settings : MyCommandSettings
    {
        [CommandOption("--data-dir")]
        public string DataDir { get; set; }

        [CommandOption("--user-file-pattern")]
        [DefaultValue("benutzer*.json")]
        public string UserFilePattern { get; set; }

        [CommandOption("--group-file-pattern")]
        [DefaultValue("gruppen*.json")]
        public string GroupFilePattern { get; set; }

        [CommandOption("--default-password")]
        [DefaultValue("Password01")]
        public string DefaultPassword { get; set; }

        [CommandOption("--dry-run")]
        [DefaultValue(false)]
        public bool DryRun { get; set; }
    }

    public DeployAadUsersAndGroupsCommand(AzureAdWorker worker)
    {
        _worker = worker;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, DeployAadUsersAndGroupsCommand.Settings settings)
    {
        await _worker.DeployAadUsersAndGroups(settings.LoginAzCli,
            settings.DataDir,
            settings.DefaultPassword,
            settings.UserFilePattern,
            settings.GroupFilePattern,
            settings.DryRun);
        return 0;
    }
}