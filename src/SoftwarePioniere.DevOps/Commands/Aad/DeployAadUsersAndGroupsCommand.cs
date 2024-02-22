using System.ComponentModel;
using System.Threading.Tasks;
using SoftwarePioniere.DevOps.Services;
using Spectre.Console.Cli;

// ReSharper disable ClassNeverInstantiated.Global

namespace SoftwarePioniere.DevOps.Commands.Aad;

public class DeployAadUsersAndGroupsCommand(AzureAdService service)
    : AadCommandBase<DeployAadUsersAndGroupsCommand.SettingsBase>
{
    public class SettingsBase : AadCommandSettingsBase
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

    public override async Task<int> ExecuteAsync(CommandContext context, DeployAadUsersAndGroupsCommand.SettingsBase settingsBase)
    {
        await service.DeployAadUsersAndGroups(settingsBase.LoginAzCli,
            settingsBase.DataDir,
            settingsBase.DefaultPassword,
            settingsBase.UserFilePattern,
            settingsBase.GroupFilePattern,
            settingsBase.DryRun);
        return 0;
    }
}