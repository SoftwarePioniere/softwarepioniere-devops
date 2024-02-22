using SoftwarePioniere.DevOps.Commands.Aad;
using Spectre.Console.Cli;

namespace SoftwarePioniere.DevOps;

public static class CommandRegistration
{
    public static void RegisterCommands(this IConfigurator config)
    {

        config.AddBranch("aad",
            aad =>
            {
                aad.AddCommand<ShowSubscriptionsCommand>("subscriptions")
                    .WithDescription("List all Azure Subscriptions");
                aad.AddCommand<ShowAadUsersAndGroupsCommand>("show")
                    .WithDescription("Parse and list the Data Files and Load From AAD");
                aad.AddCommand<ExportAadUsersAndGroupsCommand>("export")
                    .WithDescription("Export existing AAD");
                aad.AddCommand<DeployAadUsersAndGroupsCommand>("deploy")
                    .WithDescription("Deploy the AAD");
            });
    }
}