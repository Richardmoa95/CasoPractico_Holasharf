using IntegrationServices.Application.Events;
using IntegrationServices.Application.Ports;
using Microsoft.Extensions.Logging;

namespace IntegrationServices.Infrastructure.Messaging;

public sealed class InMemoryMessageBus : IMessageBus
{
    private readonly InMemoryMessageQueue _queue;
    private readonly ILogger<InMemoryMessageBus> _logger;

    public InMemoryMessageBus(InMemoryMessageQueue queue, ILogger<InMemoryMessageBus> logger)
    {
        _queue = queue;
        _logger = logger;
    }

    public async Task PublishAsync(TmsEventMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Publishing internal message. EventId: {EventId}, OrderNumber: {OrderNumber}, Status: {Status}", message.EventId, message.OrderNumber, message.Status);

        await _queue.EnqueueAsync(message, cancellationToken);
    }
}