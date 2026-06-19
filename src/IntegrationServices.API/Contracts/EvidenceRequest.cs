namespace IntegrationServices.API.Contracts;

public sealed record EvidenceRequest(
    string Label,
    string FileType,
    string FileName,
    string Url
);