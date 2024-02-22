// using System.ComponentModel;
// using System.Threading.Tasks;
// using SoftwarePioniere.DevOps.Services;
// using Spectre.Console.Cli;
//
// namespace SoftwarePioniere.DevOps.Commands.Aad;
//
// public class ExportAadUsersAndGroupsCommand(AzureAdService service)
//     : AadCommandBase<ExportAadUsersAndGroupsCommand.SettingsBase>
// {
//     public class SettingsBase : AadCommandSettingsBase
//     {
//         [CommandOption("--data-dir")]
//         public string DataDir { get; set; }
//
//         [CommandOption("--user-file-pattern")]
//         [DefaultValue("benutzer*.json")]
//         public string UserFilePattern { get; set; }
//
//         [CommandOption("--group-file-pattern")]
//         [DefaultValue("gruppen*.json")]
//         public string GroupFilePattern { get; set; }
//     }
//
//     public override async Task<int> ExecuteAsync(CommandContext context, ExportAadUsersAndGroupsCommand.SettingsBase settingsBase)
//     {
//         await service.ExportAadUsersAndGroups(settingsBase.LoginAzCli,
//             settingsBase.DataDir,
//             settingsBase.UserFilePattern,
//             settingsBase.GroupFilePattern);
//         return 0;
//     }
// }