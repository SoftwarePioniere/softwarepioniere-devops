using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace SoftwarePioniere.DevOps
{
    static class Program
    {
        static async Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand();

            {
                var aadCommand = new Command("aad");
                rootCommand.Add(aadCommand);

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
                
                var aadShowCommand = new Command("show", "Parse and list the Data Files and Load From AAD")
                {
                    loginAzCliOption,
                    dataDirOption,
                    userFilePatternOption,
                    groupFilePatternOption
                };
                aadCommand.Add(aadShowCommand);
                aadShowCommand.Handler =
                    CommandHandler.Create<bool,string, string, string>(AadCommandHandler.HandleShowCommand);

                var aadExportCommand = new Command("export", "Export existing AAD")
                {
                    loginAzCliOption,
                    dataDirOption,
                    userFilePatternOption,
                    groupFilePatternOption
                };
                aadCommand.Add(aadExportCommand);
                aadExportCommand.Handler =
                    CommandHandler.Create<bool, string, string, string>(AadCommandHandler.HandleExportCommand);

                var aadDeployCommand = new Command("deploy", "Deploy the AAD")
                {
                    loginAzCliOption,
                    dataDirOption,
                    defaultPassordOption,
                    userFilePatternOption,
                    groupFilePatternOption,
                    dryRunOption
                };
                aadCommand.Add(aadDeployCommand);
                aadDeployCommand.Handler =
                    CommandHandler.Create<bool, string, string, string, string,bool>(AadCommandHandler.HandleDeployCommand);
            }

            return await rootCommand.InvokeAsync(args);
        }
    }
}