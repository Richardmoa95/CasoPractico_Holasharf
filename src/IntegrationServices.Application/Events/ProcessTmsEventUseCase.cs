using IntegrationServices.Application.Common;
using IntegrationServices.Application.Notifications;
using IntegrationServices.Application.Ports;
using IntegrationServices.Domain.Orders;
using IntegrationServices.Domain.ValueObjects;

namespace IntegrationServices.Application.Events;

public sealed class ProcessTmsEventUseCase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderHistoryRepository _historyRepository;
    private readonly IEvidenceStorage _evidenceStorage;
    private readonly INotificationPublisher _notificationPublisher;
    private readonly IMessageBus _messageBus;
    private readonly IIdempotencyStore _idempotencyStore;

    public ProcessTmsEventUseCase(
        IOrderRepository orderRepository,
        IOrderHistoryRepository historyRepository,
        IEvidenceStorage evidenceStorage,
        INotificationPublisher notificationPublisher,
        IMessageBus messageBus,
        IIdempotencyStore idempotencyStore)
    {
        _orderRepository = orderRepository;
        _historyRepository = historyRepository;
        _evidenceStorage = evidenceStorage;
        _notificationPublisher = notificationPublisher;
        _messageBus = messageBus;
        _idempotencyStore = idempotencyStore;
    }

    public async Task<Result<ProcessTmsEventResult>> ExecuteAsync(TmsEventMessage message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(message.EventId))
        {
            return Result<ProcessTmsEventResult>.Failure("EventId is required.");
        }

        var alreadyProcessed = await _idempotencyStore.ExistsAsync(message.EventId, cancellationToken);

        if (alreadyProcessed)
        {
            return Result<ProcessTmsEventResult>.Success(
                new ProcessTmsEventResult(
                    message.OrderNumber,
                    message.Status,
                    WasProcessed: false,
                    WasDuplicated: true,
                    WasIgnoredBecauseFinalState: false,
                    ToBeReturnWasEmitted: false
                )
            );
        }

        var order = await _orderRepository.GetByOrderNumberAsync(message.OrderNumber,cancellationToken);

        if (order is null)
        {
            return Result<ProcessTmsEventResult>.Failure($"Order {message.OrderNumber} was not found.");
        }

        var tmsEvent = MapToDomainEvent(message);

        var history = OrderHistory.FromTmsEvent(tmsEvent);

        await _historyRepository.AddAsync(history, cancellationToken);

        if (order.IsFinalState())
        {
            await _idempotencyStore.MarkAsProcessedAsync(message.EventId, cancellationToken);

            return Result<ProcessTmsEventResult>.Success(new ProcessTmsEventResult
                (
                    order.OrderNumber,
                    order.Status,
                    WasProcessed: false,
                    WasDuplicated: false,
                    WasIgnoredBecauseFinalState: true,
                    ToBeReturnWasEmitted: false
                )
            );
        }

        order.ApplyTmsEvent(tmsEvent);

        if (tmsEvent.HasEvidenceMilestone() && tmsEvent.Evidences.Any())
        {
            foreach (var evidence in tmsEvent.Evidences)
            {
                var storedEvidence = await _evidenceStorage.StoreAsync(order.OrderNumber, evidence, cancellationToken);

                order.AddEvidence(storedEvidence);
            }
        }

        var toBeReturnWasEmitted = false;

        if (order.ShouldBeReturned())
        {
            order.MarkToBeReturned();

            var toBeReturnMessage = CreateToBeReturnMessage(message);

            await _messageBus.PublishAsync(toBeReturnMessage, cancellationToken);

            toBeReturnWasEmitted = true;
        }

        await _orderRepository.SaveAsync(order, cancellationToken);

        var notification = new OrderStatusNotification(
            order.OrderNumber,
            order.TrackingNumber,
            order.ClientCode,
            order.ClientName,
            order.Status,
            message.SubStatus,
            message.EventDate
        );

        await _notificationPublisher.PublishAsync(notification, cancellationToken);

        await _idempotencyStore.MarkAsProcessedAsync(message.EventId, cancellationToken);

        return Result<ProcessTmsEventResult>.Success(
            new ProcessTmsEventResult(
                order.OrderNumber,
                order.Status,
                WasProcessed: true,
                WasDuplicated: false,
                WasIgnoredBecauseFinalState: false,
                ToBeReturnWasEmitted: toBeReturnWasEmitted
            )
        );
    }

    private static TmsEvent MapToDomainEvent(TmsEventMessage message)
    {
        var evidences = message.Evidences
            .Select(evidence => Evidence.Create(evidence.Label, evidence.FileType, evidence.FileName, evidence.Url))
            .ToList();

        return new TmsEvent(
            message.ServiceType,
            message.DispatchType,
            message.Status,
            message.SubStatus,
            message.VehicleCode,
            message.CourierName,
            message.OrderNumber,
            message.TrackingNumber,
            message.ClientCode,
            message.ClientName,
            message.ReceivedBy,
            message.Comments,
            evidences,
            message.EventDate
        );
    }

    private static TmsEventMessage CreateToBeReturnMessage(TmsEventMessage originalMessage)
    {
        return originalMessage with
        {
            EventId = $"{originalMessage.OrderNumber}-TO_BE_RETURN-{DateTime.UtcNow:O}",
            Status = OrderStatus.ToBeReturn,
            SubStatus = "AUTO GENERATED AFTER 3 VISITS",
            Comments = "Order automatically marked to be returned after reaching 3 delivery visits.",
            Evidences = Array.Empty<EvidenceMessage>(),
            EventDate = DateTime.UtcNow
        };
    }
}