using IntegrationServices.Domain.ValueObjects;

namespace IntegrationServices.Application.Ports;

public interface IEvidenceStorage
{
    Task<Evidence> StoreAsync(string orderNumber, Evidence evidence, CancellationToken cancellationToken = default);
}