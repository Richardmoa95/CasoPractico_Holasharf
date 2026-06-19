using IntegrationServices.Application.Events;
using IntegrationServices.Application.Notifications;

namespace IntegrationServices.Application.Ports;

public interface IMessageBus
{
    Task PublishAsync(TmsEventMessage message, CancellationToken cancellationToken = default);
}