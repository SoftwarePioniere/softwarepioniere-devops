using System.ComponentModel;
using System.Threading.Tasks;
using Spectre.Console.Cli;

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
    private readonly SubscriptionWorker _subscriptionWorker;

    public ShowSubscriptionsCommand(SubscriptionWorker subscriptionWorker)
    {
        _subscriptionWorker = subscriptionWorker;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, MyCommandSettings settings)
    {
        await _subscriptionWorker.ListSubscriptions(settings.LoginAzCli);
        return 0;
    }
}