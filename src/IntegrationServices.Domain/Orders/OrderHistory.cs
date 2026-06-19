using IntegrationServices.Domain.Common;

namespace IntegrationServices.Domain.Orders;

public sealed class OrderHistory : EntityBase<Guid>
{
    public string OrderNumber { get; }
    public OrderStatus Status { get; }
    public string? SubStatus { get; }
    public string? Comments { get; }
    public string? CourierName { get; }
    public string? VehicleCode { get; }
    public DateTime EventDate { get; }
    public DateTime RegisteredAtUtc { get; }

    private OrderHistory(Guid id, string orderNumber, OrderStatus status, string? subStatus, string? comments, string? courierName, string? vehicleCode, DateTime eventDate, 
        DateTime registeredAtUtc) : base(id)
    {
        OrderNumber = orderNumber;
        Status = status;
        SubStatus = subStatus;
        Comments = comments;
        CourierName = courierName;
        VehicleCode = vehicleCode;
        EventDate = eventDate;
        RegisteredAtUtc = registeredAtUtc;
    }

    public static OrderHistory FromTmsEvent(TmsEvent tmsEvent)
    {
        return new OrderHistory(
            Guid.NewGuid(),
            tmsEvent.OrderNumber,
            tmsEvent.Status,
            tmsEvent.SubStatus,
            tmsEvent.Comments,
            tmsEvent.CourierName,
            tmsEvent.VehicleCode,
            tmsEvent.EventDate,
            DateTime.UtcNow
        );
    }
}