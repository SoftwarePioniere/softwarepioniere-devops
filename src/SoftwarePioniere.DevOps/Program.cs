using System;
using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace SoftwarePioniere.DevOps
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand();

            {
                var aadCommand = new Command("aad")
                {
                    new Option<bool>("--login-az-cli", "Login from az cli Service Principial, not from Environment vars"),
                    new Option<string>("--data-dir", "The Directory which contains the data files. Defaults to Current Directory"),
                    new Option<string>("--default-password",  () => "Password01", "The Default Passwort on User Creation"),
                    new Option<string>("--user-file-pattern", () => "benutzer*.json", "Pattern to read the User Files"),
                    new Option<string>("--group-file-pattern", () => "gruppen*.json", "Pattern to read the Group Files")
                    
                };
                rootCommand.Add(aadCommand);

                var aadParseCommand = new Command("parse", "Parse and list the Data Files");
                aadCommand.Add(aadParseCommand);
                aadParseCommand.Handler = CommandHandler.Create<string, string, string>(AzureActiveDirectoryCommandHandler.ParseHandler);

                var aadExportCommand = new Command("export", "Export existing AAD");
                aadCommand.Add(aadExportCommand);

                var aadDeployCommand = new Command("deploy", "Deploy the AAD");
                aadCommand.Add(aadDeployCommand);
            }

            return await rootCommand.InvokeAsync(args);
        }
    }

    static class AzureActiveDirectoryCommandHandler
    {
        public static Task<int> ParseHandler(string dataDir, string userFilePattern, string groupFilePattern)
        {
            Console.WriteLine($"{nameof(dataDir)}: {dataDir}");
            Console.WriteLine($"{nameof(userFilePattern)}: {userFilePattern}");
            Console.WriteLine($"{nameof(groupFilePattern)}: {groupFilePattern}");

            return Task.FromResult(0);
        }
    }
}