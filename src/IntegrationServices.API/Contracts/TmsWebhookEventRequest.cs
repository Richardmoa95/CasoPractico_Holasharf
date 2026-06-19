namespace IntegrationServices.API.Contracts;

public sealed record TmsWebhookEventRequest(
    string ServiceType,
    string DispatchType,
    string Status,
    string? SubStatus,
    string? VehicleCode,
    string? CourierName,
    TmsEventDetailsRequest Details,
    string EventDate
);