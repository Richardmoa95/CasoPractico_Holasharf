using IntegrationServices.Domain.Orders;

namespace IntegrationServices.Application.Events;

public sealed record TmsEventMessage(
    string EventId,
    ServiceType ServiceType,
    DispatchType DispatchType,
    OrderStatus Status,
    string? SubStatus,
    string? VehicleCode,
    string? CourierName,
    string OrderNumber,
    string TrackingNumber,
    string ClientCode,
    string ClientName,
    string? ReceivedBy,
    string? Comments,
    IReadOnlyCollection<EvidenceMessage> Evidences,
    DateTime EventDate
);