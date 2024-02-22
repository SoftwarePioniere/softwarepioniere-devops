using System.Threading.Tasks;
using SoftwarePioniere.DevOps.Services;
using Spectre.Console.Cli;

namespace SoftwarePioniere.DevOps.Commands.Aad;

public class ShowSubscriptionsCommand(AzureSubscriptionService service) : AadCommandBase<AadCommandSettingsBase>
{
    public override async Task<int> ExecuteAsync(CommandContext context, AadCommandSettingsBase settingsBase)
    {
        await service.ListSubscriptions(settingsBase.LoginAzCli);
        return 0;
    }
}