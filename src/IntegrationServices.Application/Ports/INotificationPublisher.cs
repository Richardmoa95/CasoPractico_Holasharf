using IntegrationServices.Application.Notifications;

namespace IntegrationServices.Application.Ports;

public interface INotificationPublisher
{
    Task PublishAsync(OrderStatusNotification notification, CancellationToken cancellationToken = default);
}