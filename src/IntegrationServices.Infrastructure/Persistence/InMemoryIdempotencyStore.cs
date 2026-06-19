using IntegrationServices.Application.Ports;
using System.Collections.Concurrent;

namespace IntegrationServices.Infrastructure.Persistence;

public sealed class InMemoryIdempotencyStore : IIdempotencyStore
{
    private readonly ConcurrentDictionary<string, DateTime> _processedEvents = new();

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_processedEvents.ContainsKey(key));
    }

    public Task MarkAsProcessedAsync(string key, CancellationToken cancellationToken = default)
    {
        _processedEvents.TryAdd(key, DateTime.UtcNow);

        return Task.CompletedTask;
    }
}
