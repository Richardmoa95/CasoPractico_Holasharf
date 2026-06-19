using IntegrationServices.Application.Events;
using System.Threading.Channels;

namespace IntegrationServices.Infrastructure.Messaging;

public sealed class InMemoryMessageQueue
{
    private readonly Channel<TmsEventMessage> _channel = Channel.CreateUnbounded<TmsEventMessage>();

    public async Task EnqueueAsync(TmsEventMessage message, CancellationToken cancellationToken = default)
    {
        await _channel.Writer.WriteAsync(message, cancellationToken);
    }

    public async Task<TmsEventMessage> DequeueAsync(CancellationToken cancellationToken = default) 
    {
        return await _channel.Reader.ReadAsync(cancellationToken);
    }

    public IAsyncEnumerable<TmsEventMessage> ReadAllAsync(CancellationToken cancellationToken = default)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
}
