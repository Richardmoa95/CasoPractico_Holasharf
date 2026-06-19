namespace IntegrationServices.API.Contracts;

public sealed record TmsEventDetailsRequest(
    string OrderNumber,
    string TrackingNumber,
    string ClientCode,
    string ClientName,
    string? ReceivedBy,
    string? Comments,
    IReadOnlyCollection<EvidenceRequest>? Evidences
);