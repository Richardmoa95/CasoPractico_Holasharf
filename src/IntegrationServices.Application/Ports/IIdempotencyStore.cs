namespace IntegrationServices.Application.Ports;

public interface IIdempotencyStore
{
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    Task MarkAsProcessedAsync(string key, CancellationToken cancellationToken = default);
}