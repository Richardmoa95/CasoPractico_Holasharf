using IntegrationServices.Domain.Orders;

namespace IntegrationServices.Application.Ports;

public interface IOrderHistoryRepository
{
    Task AddAsync(OrderHistory history, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<OrderHistory>> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
}