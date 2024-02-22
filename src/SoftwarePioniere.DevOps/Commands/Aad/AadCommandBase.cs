using Spectre.Console.Cli;

namespace SoftwarePioniere.DevOps.Commands.Aad;

public abstract class AadCommandBase<T> : AsyncCommand<T> where T : AadCommandSettingsBase
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