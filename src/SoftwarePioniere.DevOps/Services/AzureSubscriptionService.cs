// using System.Threading.Tasks;
// using Microsoft.Extensions.Logging;
//
// namespace SoftwarePioniere.DevOps.Services;
//
// public class AzureSubscriptionService(ILoggerFactory loggerFactory)
// {
//     private readonly ILogger _logger = loggerFactory.CreateLogger("SUBSCRIPTIONS");
//
//
//     public async Task ListSubscriptions(bool loginAzCli)
//     {
//         _logger.LogInformation("{Operation} {LoginWithAzCli}", nameof(ListSubscriptions), loginAzCli);
//         var auth = AzureUtils.Login(loginAzCli);
//
//         {
//             var items = await AzureUtils.LoadPagedCollectionAsync(auth.Tenants.ListAsync());
//             _logger.LogInformation("===========================================");
//             _logger.LogInformation("Tenants");
//             _logger.LogInformation("===========================================");
//             foreach (var x in items) _logger.LogInformation("{TenantId}", x.TenantId);
//         }
//         {
//             var items = await AzureUtils.LoadPagedCollectionAsync(auth.Subscriptions.ListAsync());
//
//             _logger.LogInformation("===========================================");
//             _logger.LogInformation("Subscriptions");
//             _logger.LogInformation("===========================================");
//             foreach (var x in items) _logger.LogInformation("{Name} | {Id}", x.DisplayName, x.SubscriptionId);
//         }
//
//     }
// }