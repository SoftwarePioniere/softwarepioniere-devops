using Spectre.Console.Cli;

namespace SoftwarePioniere.DevOps;

public class AppWorkerParams
{
    public string[] Args { get; set; }
    public CommandApp App { get; set; }
}