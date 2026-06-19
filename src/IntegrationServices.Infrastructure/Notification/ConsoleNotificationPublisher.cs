using IntegrationServices.Application.Notifications;
using IntegrationServices.Application.Ports;
using Microsoft.Extensions.Logging;

namespace IntegrationServices.Infrastructure.Notification;

public sealed class ConsoleNotificationPublisher : INotificationPublisher
{
    private readonly ILogger<ConsoleNotificationPublisher> _logger;

    public ConsoleNotificationPublisher(ILogger<ConsoleNotificationPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync(OrderStatusNotification notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Publishing notification. ClientCode: {ClientCode}, OrderNumber: {OrderNumber}, Status: {Status}, SubStatus: {SubStatus}", 
            notification.ClientCode,
            notification.OrderNumber,
            notification.Status,
            notification.SubStatus
        );

        return Task.CompletedTask;
    }
}