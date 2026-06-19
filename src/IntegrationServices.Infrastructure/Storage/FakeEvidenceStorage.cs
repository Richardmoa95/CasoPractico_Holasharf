using IntegrationServices.Application.Ports;
using IntegrationServices.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace IntegrationServices.Infrastructure.Storage;

public sealed class FakeEvidenceStorage : IEvidenceStorage
{
    private readonly ILogger<FakeEvidenceStorage> _logger;

    public FakeEvidenceStorage(ILogger<FakeEvidenceStorage> logger)
    {
        _logger = logger;
    }

    public Task<Evidence> StoreAsync(string orderNumber, Evidence evidence, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Storing evidence. OrderNumber: {OrderNumber}, FileName: {FileName}, SourceUrl: {SourceUrl}", orderNumber, evidence.FileName, evidence.SourceUrl);

        var storedUrl = $"cloud://evidences/{orderNumber}/{Guid.NewGuid()}-{evidence.FileName}";

        var storedEvidence = evidence.MarkAsStored(storedUrl);

        return Task.FromResult(storedEvidence);
    }
}