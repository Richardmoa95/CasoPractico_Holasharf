using Microsoft.Extensions.DependencyInjection;

namespace IntegrationServices.Worker;

public static class DependencyInjection
{
    public static IServiceCollection AddWorker(this IServiceCollection services)
    {
        services.AddHostedService<TmsEventBackgroundWorker>();

        return services;
    }
}