using System.ComponentModel;
using Spectre.Console.Cli;

namespace SoftwarePioniere.DevOps.Commands.Aad;

public class AadCommandSettingsBase : CommandSettings
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