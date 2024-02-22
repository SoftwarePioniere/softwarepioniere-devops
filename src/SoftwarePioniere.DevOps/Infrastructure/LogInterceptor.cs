using Serilog.Core;
using SoftwarePioniere.DevOps.Commands;
using Spectre.Console.Cli;

namespace SoftwarePioniere.DevOps.Infrastructure;

public class LogInterceptor : ICommandInterceptor
{
    public static readonly LoggingLevelSwitch LogLevel = new();

    public void Intercept(CommandContext context, CommandSettings settings)
    {

        if (settings is LogCommandSettings logSettings)
        {
            LogLevel.MinimumLevel = logSettings.LogLevel;
        }
    }
}