using IntegrationServices.Application.Events;
using IntegrationServices.Infrastructure.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;

namespace IntegrationServices.Worker;

public sealed class TmsEventBackgroundWorker : BackgroundService
{
    private readonly InMemoryMessageQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly DeadLetterQueue _deadLetterQueue;
    private readonly ILogger<TmsEventBackgroundWorker> _logger;

    private readonly IAsyncPolicy _retryPolicy;

    public TmsEventBackgroundWorker(InMemoryMessageQueue queue, IServiceScopeFactory scopeFactory, DeadLetterQueue deadLetterQueue, ILogger<TmsEventBackgroundWorker> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _deadLetterQueue = deadLetterQueue;
        _logger = logger;

        _retryPolicy = Policy.Handle<Exception>().WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
            onRetry: (exception, delay, attempt, context) =>
            {
                _logger.LogWarning(exception, "Retrying TMS event processing. Attempt: {Attempt}, Delay: {DelaySeconds}s", attempt, delay.TotalSeconds);
            }
        );
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TMS Event Background Worker started.");

        await foreach (var message in _queue.ReadAllAsync(stoppingToken))
        {
            await ProcessMessageSafelyAsync(message, stoppingToken);
        }
    }

    private async Task ProcessMessageSafelyAsync(TmsEventMessage message, CancellationToken cancellationToken)
    {
        try
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                using var scope = _scopeFactory.CreateScope();

                var useCase = scope.ServiceProvider.GetRequiredService<ProcessTmsEventUseCase>();

                var result = await useCase.ExecuteAsync(message, cancellationToken);

                if (!result.IsSuccess)
                {
                    throw new InvalidOperationException(result.Error);
                }

                var value = result.Value!;

                if (value.WasDuplicated)
                {
                    _logger.LogInformation("Duplicated event ignored. EventId: {EventId}, OrderNumber: {OrderNumber}", message.EventId, message.OrderNumber);

                    return;
                }

                if (value.WasIgnoredBecauseFinalState)
                {
                    _logger.LogInformation("Event ignored because order is already in final state. OrderNumber: {OrderNumber}, CurrentStatus: {Status}", value.OrderNumber, value.Status);

                    return;
                }

                _logger.LogInformation("TMS event processed successfully. OrderNumber: {OrderNumber}, Status: {Status}, ToBeReturnWasEmitted: {ToBeReturnWasEmitted}", value.OrderNumber, value.Status, value.ToBeReturnWasEmitted);
            });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("TMS event processing was cancelled. EventId: {EventId}", message.EventId);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "TMS event processing failed after retries. EventId: {EventId}, OrderNumber: {OrderNumber}", message.EventId, message.OrderNumber);

            _deadLetterQueue.Add(message, exception);
        }
    }
}