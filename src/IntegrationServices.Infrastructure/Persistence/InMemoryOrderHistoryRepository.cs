using IntegrationServices.Application.Ports;
using IntegrationServices.Domain.Orders;
using System.Collections.Concurrent;

namespace IntegrationServices.Infrastructure.Persistence;

public sealed class InMemoryOrderHistoryRepository : IOrderHistoryRepository
{
    private readonly ConcurrentDictionary<string, List<OrderHistory>> _history = new();

    public Task AddAsync(OrderHistory history, CancellationToken cancellationToken = default)
    {
        var list = _history.GetOrAdd(history.OrderNumber, _ => new List<OrderHistory>());

        lock (list)
        {
            list.Add(history);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<OrderHistory>> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        if (!_history.TryGetValue(orderNumber, out var list))
        {
            return Task.FromResult<IReadOnlyCollection<OrderHistory>>(Array.Empty<OrderHistory>());
        }

        lock (list)
        {
            return Task.FromResult<IReadOnlyCollection<OrderHistory>>(list.OrderBy(x => x.EventDate).ToList());
        }
    }
}