using IntegrationServices.Application.Events;
using System.Collections.Concurrent;

namespace IntegrationServices.Infrastructure.Messaging;

public sealed record DeadLetterMessage(TmsEventMessage Message, string Error, DateTime FailedAtUtc);

public sealed class DeadLetterQueue
{
    private readonly ConcurrentQueue<DeadLetterMessage> _messages = new();

    public void Add(TmsEventMessage message, Exception exception)
    {
        _messages.Enqueue(new DeadLetterMessage(message, exception.Message, DateTime.UtcNow));
    }

    public IReadOnlyCollection<DeadLetterMessage> GetAll()
    {
        return _messages.ToArray();
    }
}