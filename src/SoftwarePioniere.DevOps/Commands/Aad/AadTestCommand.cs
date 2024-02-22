using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Spectre.Console.Cli;

namespace SoftwarePioniere.DevOps.Commands.Aad;

public class AadTestCommand : AadCommandBase<AadTestCommand.Settings>
{
    private readonly ILogger _logger;

    public AadTestCommand(ILogger<AadTestCommand> logger)
    {
        _logger = logger;
        _logger.LogDebug("{0} initialized", nameof(AadTestCommand));
    }

    public class Settings : AadCommandSettingsBase
    {
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        _logger.LogInformation("Test Run");

        string[] scopes = { "https://graph.microsoft.com/.default" };

        var clientSecretCredential =
            new ClientSecretCredential(settings.TenantId, settings.ClientId, settings.ClientSecret);

        var graphClient = new GraphServiceClient(clientSecretCredential, scopes);

        var users = (await graphClient.Users.GetAsync())?.Value;
        if (users != null)
        {
            foreach (var user in users)
            {
                _logger.LogDebug("User: {GivenName}: {Surname}", user.GivenName, user.Surname);
            }
        }


        return 0;
    }

    // private async Task<T[]> LoadPagedCollectionAsync<T>(Task<IParsable> listAsync) where T : IParsable<T>?
    // {
    //      var list = new List<T>();
    //      var page = await listAsync;
    //
    //      while (page != null)
    //      {
    //          list.AddRange(page);
    //          page = await page.GetNextPageAsync();
    //      }
    //
    //      return list.ToArray();
    //  }
}