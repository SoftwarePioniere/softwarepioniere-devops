using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SoftwarePioniere.DevOps;

public class SubscriptionWorker
{
    private readonly ILogger _logger;

    public SubscriptionWorker(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger("SUBSCRIPTIONS");
    }


    public async Task ListSubscriptions(bool loginAzCli)
    {
        _logger.LogInformation("{Operation} {LoginWithAzCli}", nameof(ListSubscriptions), loginAzCli);
        var auth = MyAzure.Login(loginAzCli);
        
        {
            var items = await MyAzure.LoadPagedCollectionAsync(auth.Tenants.ListAsync());
            _logger.LogInformation("===========================================");
            _logger.LogInformation("Tenants");
            _logger.LogInformation("===========================================");
            foreach (var x in items) _logger.LogInformation("{TenantId}", x.TenantId);
        }
        {
            var items = await MyAzure.LoadPagedCollectionAsync(auth.Subscriptions.ListAsync());
        
            _logger.LogInformation("===========================================");
            _logger.LogInformation("Subscriptions");
            _logger.LogInformation("===========================================");
            foreach (var x in items) _logger.LogInformation("{Name} | {Id}", x.DisplayName, x.SubscriptionId);
        }
      
       
    }
}