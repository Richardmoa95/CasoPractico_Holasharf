using IntegrationServices.Application.Ports;
using IntegrationServices.Domain.Orders;
using System.Collections.Concurrent;

namespace IntegrationServices.Infrastructure.Persistence;

public sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly ConcurrentDictionary<string, Order> _orders = new();

    public InMemoryOrderRepository()
    {
        Seed();
    }

    public Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        _orders.TryGetValue(orderNumber, out var order);
        return Task.FromResult(order);
    }

    public Task SaveAsync(Order order, CancellationToken cancellationToken = default)
    {
        _orders[order.OrderNumber] = order;
        return Task.CompletedTask;
    }

    public IReadOnlyCollection<Order> GetAll()
    {
        return _orders.Values.ToList();
    }

    private void Seed()
    {
        var order = Order.Create(
            orderNumber: "2500000006-01",
            trackingNumber: "OE2500000006-01",
            clientCode: "01021755",
            clientName: "TIENDAS PERUANAS S.A."
        );

        _orders.TryAdd(order.OrderNumber, order);
    }
}
