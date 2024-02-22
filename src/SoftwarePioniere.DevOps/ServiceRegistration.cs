using Microsoft.Extensions.DependencyInjection;
using SoftwarePioniere.DevOps.Services;

namespace SoftwarePioniere.DevOps;

internal static class ServiceRegistration
{
    public static IServiceCollection RegisterServices(this IServiceCollection services)
    {
        services
            .AddSingleton<AzureSubscriptionService>()
            .AddSingleton<AzureAdService>()
            ;

        return services;
    }
}