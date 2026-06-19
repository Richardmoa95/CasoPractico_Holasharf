using IntegrationServices.Application.Ports;
using IntegrationServices.Infrastructure.Messaging;
using IntegrationServices.Infrastructure.Notification;
using IntegrationServices.Infrastructure.Persistence;
using IntegrationServices.Infrastructure.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationServices.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<InMemoryMessageQueue>();
        services.AddSingleton<DeadLetterQueue>();

        services.AddSingleton<IMessageBus, InMemoryMessageBus>();

        services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
        services.AddSingleton<IOrderHistoryRepository, InMemoryOrderHistoryRepository>();
        services.AddSingleton<IIdempotencyStore, InMemoryIdempotencyStore>();

        services.AddSingleton<IEvidenceStorage, FakeEvidenceStorage>();
        services.AddSingleton<INotificationPublisher, ConsoleNotificationPublisher>();

        return services;
    }
}
