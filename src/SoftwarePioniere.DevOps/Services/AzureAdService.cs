using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SoftwarePioniere.DevOps.Services;

public class AzureAdService(ILoggerFactory loggerFactory)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger("AAD");

    public async Task ShowAadUsersAndGroups(bool loginAzCli, string dataDir, string userFilePattern,
        string groupFilePattern)
    {
        _logger.LogInformation("{Operation} {LoginWithAzCli}", nameof(ShowAadUsersAndGroups), loginAzCli);
        await AzureUtils.ShowAadUsersAndGroups(loginAzCli, dataDir, userFilePattern, groupFilePattern);
    }

    public async Task ExportAadUsersAndGroups(bool loginAzCli, string dataDir, string userFilePattern,
        string groupFilePattern)
    {
        _logger.LogInformation("{Operation} {LoginWithAzCli}", nameof(ExportAadUsersAndGroups), loginAzCli);
        await AzureUtils.ExportAadUsersAndGroups(loginAzCli, dataDir, userFilePattern, groupFilePattern);
    }

    public async Task DeployAadUsersAndGroups(bool loginAzCli, string dataDir, string defaultPassword,
        string userFilePattern,
        string groupFilePattern, bool dryRun)
    {
        _logger.LogInformation("{Operation} {LoginWithAzCli}", nameof(DeployAadUsersAndGroups), loginAzCli);
        await AzureUtils.DeployAadUsersAndGroups(loginAzCli,
            dataDir,
            defaultPassword,
            userFilePattern,
            groupFilePattern,
            dryRun);
    }
}