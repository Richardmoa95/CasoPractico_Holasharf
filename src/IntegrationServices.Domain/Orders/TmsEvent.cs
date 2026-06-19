using IntegrationServices.Domain.Common;
using IntegrationServices.Domain.ValueObjects;

namespace IntegrationServices.Domain.Orders;

public sealed class TmsEvent
{
    public ServiceType ServiceType { get; }
    public DispatchType DispatchType { get; }
    public OrderStatus Status { get; }
    public string? SubStatus { get; }
    public string? VehicleCode { get; }
    public string? CourierName { get; }
    public string OrderNumber { get; }
    public string TrackingNumber { get; }
    public string ClientCode { get; }
    public string ClientName { get; }
    public string? ReceivedBy { get; }
    public string? Comments { get; }
    public IReadOnlyCollection<Evidence> Evidences { get; }
    public DateTime EventDate { get; }

    public TmsEvent(ServiceType serviceType, DispatchType dispatchType, OrderStatus status, string? subStatus, string? vehicleCode, string? courierName, 
        string orderNumber, string trackingNumber, string clientCode, string clientName, string? receivedBy, string? comments, IReadOnlyCollection<Evidence> evidences, DateTime eventDate)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
        {
            throw new DomainException("Order number is required.");
        }

        if (string.IsNullOrWhiteSpace(trackingNumber))
        {
            throw new DomainException("Tracking number is required.");
        }

        if (string.IsNullOrWhiteSpace(clientCode))
        {
            throw new DomainException("Client code is required.");
        }

        if (string.IsNullOrWhiteSpace(clientName))
        {
            throw new DomainException("Client name is required.");
        }

        ServiceType = serviceType;
        DispatchType = dispatchType;
        Status = status;
        SubStatus = subStatus;
        VehicleCode = vehicleCode;
        CourierName = courierName;
        OrderNumber = orderNumber;
        TrackingNumber = trackingNumber;
        ClientCode = clientCode;
        ClientName = clientName;
        ReceivedBy = receivedBy;
        Comments = comments;
        Evidences = evidences;
        EventDate = eventDate;
    }

    public bool ShouldIncreaseVisitCounter()
    {
        return Status is OrderStatus.Delivered or OrderStatus.NotDelivered;
    }

    public bool HasEvidenceMilestone()
    {
        return Status is OrderStatus.Collected
            or OrderStatus.NotCollected
            or OrderStatus.Delivered
            or OrderStatus.NotDelivered
            or OrderStatus.Returned
            or OrderStatus.NotReturned;
    }
}