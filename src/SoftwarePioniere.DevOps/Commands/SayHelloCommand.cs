using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SoftwarePioniere.DevOps.Commands;

public class HelloCommand : Command<HelloCommand.Settings>
{
    private readonly ILogger<HelloCommand> _logger;
    private readonly IAnsiConsole _console;

    public HelloCommand(IAnsiConsole console, ILogger<HelloCommand> logger)
    {
        _console = console;
        _logger = logger;
        _logger.LogDebug("{0} initialized", nameof(HelloCommand));
    }

    public class Settings : LogCommandSettings
    {
        [CommandArgument(0, "[Name]")] public string Name { get; set; }
    }


    public override int Execute(CommandContext context, Settings settings)
    {
        _logger.LogInformation("Starting HelloCommand");
        _logger.LogDebug("Starting HelloCommand as Debug");
        AnsiConsole.MarkupLine($"Hello, [blue]{settings.Name}[/]");
        _logger.LogInformation("Completed HelloCommand");

        return 0;
    }
}