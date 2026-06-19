using IntegrationServices.Application.Ports;
using Microsoft.AspNetCore.Mvc;

namespace IntegrationServices.API.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderHistoryRepository _historyRepository;

    public OrdersController(IOrderRepository orderRepository, IOrderHistoryRepository historyRepository)
    {
        _orderRepository = orderRepository;
        _historyRepository = historyRepository;
    }

    [HttpGet("{orderNumber}")]
    public async Task<IActionResult> GetOrder(string orderNumber, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByOrderNumberAsync(orderNumber, cancellationToken);

        if (order is null)
        {
            return NotFound(new
            {
                message = $"Order {orderNumber} was not found."
            });
        }

        return Ok(new
        {
            order.OrderNumber,
            order.TrackingNumber,
            order.ClientCode,
            order.ClientName,
            status = order.Status.ToString(),
            visits = order.VisitCounter.Value,
            order.LastUpdatedAtUtc,
            evidences = order.Evidences.Select(e => new
            {
                e.Label,
                e.FileType,
                e.FileName,
                e.SourceUrl,
                e.StoredUrl
            })
        });
    }

    [HttpGet("{orderNumber}/history")]
    public async Task<IActionResult> GetHistory(string orderNumber, CancellationToken cancellationToken)
    {
        var history = await _historyRepository.GetByOrderNumberAsync(orderNumber, cancellationToken);

        return Ok(history.Select(h => new
        {
            h.Id,
            h.OrderNumber,
            status = h.Status.ToString(),
            h.SubStatus,
            h.Comments,
            h.CourierName,
            h.VehicleCode,
            h.EventDate,
            h.RegisteredAtUtc
        }));
    }
}