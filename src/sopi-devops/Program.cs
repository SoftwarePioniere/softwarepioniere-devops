using System.CommandLine;
using System.Threading.Tasks;

namespace SoftwarePioniere.DevOps
{
    static class Program
    {
        static async Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand();
            rootCommand.AppendAddCommand();
            {
                var cmd = new Command("subscriptions");
                rootCommand.Add(cmd);

                var loginAzCliOption = new Option<bool>("--login-az-cli",
                    "Login from az cli Service Principial, not from Environment vars");

                var list = new Command("list")
                {
                    loginAzCliOption
                };
                list.SetHandler((bool loginAzCliOption) => MyAzure.ListSubscriptions(loginAzCliOption),
                    loginAzCliOption);
                cmd.Add(list);
            }
            return await rootCommand.InvokeAsync(args);
        }


        static void AppendAddCommand(this Command rootCommand)
        {
            var cmd = new Command("aad");
            rootCommand.Add(cmd);

            var tempdir = @"c:\temp\aad-dev";

            var loginAzCliOption = new Option<bool>("--login-az-cli",
                "Login from az cli Service Principial, not from Environment vars");
            var dataDirOption = new Option<string>("--data-dir",
                () => tempdir,
                "The Directory which contains the data files. Defaults to Current Directory");
            var defaultPassordOption = new Option<string>("--default-password",
                () => "Password01",
                "The Default Passwort on User Creation");
            var userFilePatternOption = new Option<string>("--user-file-pattern",
                () => "benutzer*.json",
                "Pattern to read the User Files");
            var groupFilePatternOption = new Option<string>("--group-file-pattern",
                () => "gruppen*.json",
                "Pattern to read the Group Files");
            var dryRunOption = new Option<bool>("--dry-run",
                "Just dry run. No changes");

            var show = new Command("show", "Parse and list the Data Files and Load From AAD")
            {
                loginAzCliOption,
                dataDirOption,
                userFilePatternOption,
                groupFilePatternOption
            };
            cmd.Add(show);
            show.SetHandler((bool loginAzCli, string dataDir, string userFilePattern,
                    string groupFilePattern)
                => MyAzure.ShowAadUsersAndGroups(loginAzCli, dataDir, userFilePattern, groupFilePattern));

            var export = new Command("export", "Export existing AAD")
            {
                loginAzCliOption,
                dataDirOption,
                userFilePatternOption,
                groupFilePatternOption
            };
            cmd.Add(export);
            export.SetHandler((bool loginAzCli, string dataDir, string userFilePattern,
                    string groupFilePattern) =>
                MyAzure.ExportAadUsersAndGroups(loginAzCli, dataDir, userFilePattern, groupFilePattern));

            var deploy = new Command("deploy", "Deploy the AAD")
            {
                loginAzCliOption,
                dataDirOption,
                defaultPassordOption,
                userFilePatternOption,
                groupFilePatternOption,
                dryRunOption
            };
            cmd.Add(deploy);
            deploy.SetHandler((bool loginAzCli, string dataDir, string defaultPassword,
                    string userFilePattern, string groupFilePattern, bool dryRun) =>
                MyAzure
                    .DeployAadUsersAndGroups(loginAzCli,
                        dataDir,
                        defaultPassword,
                        userFilePattern,
                        groupFilePattern,
                        dryRun));
        }
    }
}