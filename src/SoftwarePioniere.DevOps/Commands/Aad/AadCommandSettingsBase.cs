using System.ComponentModel;
using Spectre.Console.Cli;

namespace SoftwarePioniere.DevOps.Commands.Aad;

public class AadCommandSettingsBase : LogCommandSettings
{
    [CommandOption("--client-id")]
    public string ClientId { get; set; }

    [CommandOption("--client-secret")] public string ClientSecret { get; set; }

    [CommandOption("--tenant-id")] public string TenantId { get; set; }

    // // [CommandOption("--url")]
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