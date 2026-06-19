namespace IntegrationServices.Application.Events;

public sealed record EvidenceMessage(
    string Label,
    string FileType,
    string FileName,
    string Url
);