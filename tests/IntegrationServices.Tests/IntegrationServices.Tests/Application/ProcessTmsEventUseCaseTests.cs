using FluentAssertions;
using IntegrationServices.Application.Events;
using IntegrationServices.Application.Ports;
using IntegrationServices.Domain.Orders;
using IntegrationServices.Domain.ValueObjects;
using NSubstitute;

namespace IntegrationServices.Tests.Application;

public sealed class ProcessTmsEventUseCaseTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderHistoryRepository _historyRepository;
    private readonly IEvidenceStorage _evidenceStorage;
    private readonly INotificationPublisher _notificationPublisher;
    private readonly IMessageBus _messageBus;
    private readonly IIdempotencyStore _idempotencyStore;

    private readonly ProcessTmsEventUseCase _useCase;

    public ProcessTmsEventUseCaseTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _historyRepository = Substitute.For<IOrderHistoryRepository>();
        _evidenceStorage = Substitute.For<IEvidenceStorage>();
        _notificationPublisher = Substitute.For<INotificationPublisher>();
        _messageBus = Substitute.For<IMessageBus>();
        _idempotencyStore = Substitute.For<IIdempotencyStore>();

        _useCase = new ProcessTmsEventUseCase(
            _orderRepository,
            _historyRepository,
            _evidenceStorage,
            _notificationPublisher,
            _messageBus,
            _idempotencyStore);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Update_Order_Status()
    {
        var order = CreateOrder();
        var message = CreateMessage(OrderStatus.Started);

        _idempotencyStore.ExistsAsync(message.EventId, Arg.Any<CancellationToken>()).Returns(false);

        _orderRepository.GetByOrderNumberAsync(message.OrderNumber, Arg.Any<CancellationToken>()).Returns(order);

        var result = await _useCase.ExecuteAsync(message);

        result.IsSuccess.Should().BeTrue();
        result.Value!.WasProcessed.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Started);

        await _orderRepository.Received(1).SaveAsync(order, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_Should_Register_History_For_Every_Event()
    {
        var order = CreateOrder();
        var message = CreateMessage(OrderStatus.Started);

        _idempotencyStore.ExistsAsync(message.EventId, Arg.Any<CancellationToken>()).Returns(false);

        _orderRepository.GetByOrderNumberAsync(message.OrderNumber, Arg.Any<CancellationToken>()).Returns(order);

        await _useCase.ExecuteAsync(message);

        await _historyRepository.Received(1).AddAsync(Arg.Any<OrderHistory>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_Should_Ignore_Duplicated_Event()
    {
        var message = CreateMessage(OrderStatus.Started);

        _idempotencyStore.ExistsAsync(message.EventId, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _useCase.ExecuteAsync(message);

        result.IsSuccess.Should().BeTrue();
        result.Value!.WasDuplicated.Should().BeTrue();
        result.Value.WasProcessed.Should().BeFalse();

        await _orderRepository.DidNotReceive().SaveAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_Should_Ignore_Order_When_Already_In_Final_State()
    {
        var order = CreateOrder();
        var deliveredMessage = CreateMessage(OrderStatus.Delivered);
        var newMessage = CreateMessage(OrderStatus.Started, eventId: "event-2");

        order.ApplyTmsEvent(ToDomainEvent(deliveredMessage));

        _idempotencyStore.ExistsAsync(newMessage.EventId, Arg.Any<CancellationToken>()).Returns(false);

        _orderRepository.GetByOrderNumberAsync(newMessage.OrderNumber, Arg.Any<CancellationToken>()).Returns(order);

        var result = await _useCase.ExecuteAsync(newMessage);

        result.IsSuccess.Should().BeTrue();
        result.Value!.WasIgnoredBecauseFinalState.Should().BeTrue();
        result.Value.WasProcessed.Should().BeFalse();

        await _orderRepository.DidNotReceive().SaveAsync(order, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_Should_Increase_VisitCounter_When_Status_Is_NotDelivered()
    {
        var order = CreateOrder();
        var message = CreateMessage(OrderStatus.NotDelivered);

        _idempotencyStore.ExistsAsync(message.EventId, Arg.Any<CancellationToken>()).Returns(false);

        _orderRepository.GetByOrderNumberAsync(message.OrderNumber, Arg.Any<CancellationToken>()).Returns(order);

        var result = await _useCase.ExecuteAsync(message);

        result.IsSuccess.Should().BeTrue();
        order.VisitCounter.Value.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Emit_ToBeReturn_When_VisitCounter_Reaches_Three()
    {
        var order = CreateOrder();

        order.ApplyTmsEvent(ToDomainEvent(CreateMessage(OrderStatus.NotDelivered, eventId: "visit-1")));
        order.ApplyTmsEvent(ToDomainEvent(CreateMessage(OrderStatus.NotDelivered, eventId: "visit-2")));

        var message = CreateMessage(OrderStatus.NotDelivered, eventId: "visit-3");

        _idempotencyStore.ExistsAsync(message.EventId, Arg.Any<CancellationToken>()).Returns(false);

        _orderRepository.GetByOrderNumberAsync(message.OrderNumber, Arg.Any<CancellationToken>()).Returns(order);

        var result = await _useCase.ExecuteAsync(message);

        result.IsSuccess.Should().BeTrue();
        result.Value!.ToBeReturnWasEmitted.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.ToBeReturn);
        order.VisitCounter.Value.Should().Be(3);

        await _messageBus.Received(1).PublishAsync(Arg.Is<TmsEventMessage>(x => x.Status == OrderStatus.ToBeReturn), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_Should_Store_Evidences_For_Evidence_Milestones()
    {
        var order = CreateOrder();

        var message = CreateMessage(
            status: OrderStatus.Collected,
            evidences: new[]
            {
                new EvidenceMessage(
                    "Paquete",
                    ".jpg",
                    "collected.jpg",
                    "https://beetrack.com/img/collected.jpg")
            });

        _idempotencyStore.ExistsAsync(message.EventId, Arg.Any<CancellationToken>()).Returns(false);

        _orderRepository.GetByOrderNumberAsync(message.OrderNumber, Arg.Any<CancellationToken>()).Returns(order);

        _evidenceStorage.StoreAsync(order.OrderNumber, Arg.Any<Evidence>(), Arg.Any<CancellationToken>()).Returns(call => 
        {
            var evidence = call.ArgAt<Evidence>(1);
            return evidence.MarkAsStored("cloud://stored/collected.jpg");
        });

        var result = await _useCase.ExecuteAsync(message);

        result.IsSuccess.Should().BeTrue();
        order.Evidences.Should().HaveCount(1);
        order.Evidences.First().StoredUrl.Should().Be("cloud://stored/collected.jpg");

        await _evidenceStorage.Received(1).StoreAsync(order.OrderNumber, Arg.Any<Evidence>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_Should_Return_Failure_When_Order_Does_Not_Exist()
    {
        var message = CreateMessage(OrderStatus.Started);

        _idempotencyStore.ExistsAsync(message.EventId, Arg.Any<CancellationToken>()).Returns(false);

        _orderRepository.GetByOrderNumberAsync(message.OrderNumber, Arg.Any<CancellationToken>()).Returns((Order?)null);

        var result = await _useCase.ExecuteAsync(message);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be($"Order {message.OrderNumber} was not found.");
    }

    private static Order CreateOrder()
    {
        return Order.Create(
            orderNumber: "2500000006-01",
            trackingNumber: "OE2500000006-01",
            clientCode: "01021755",
            clientName: "TIENDAS PERUANAS S.A.");
    }

    private static TmsEventMessage CreateMessage(OrderStatus status, string eventId = "event-1", IReadOnlyCollection<EvidenceMessage>? evidences = null)
    {
        return new TmsEventMessage(
            EventId: eventId,
            ServiceType: ServiceType.LastMile,
            DispatchType: DispatchType.HomeDelivery,
            Status: status,
            SubStatus: null,
            VehicleCode: "LIMURB06VAN",
            CourierName: "Conductor 46",
            OrderNumber: "2500000006-01",
            TrackingNumber: "OE2500000006-01",
            ClientCode: "01021755",
            ClientName: "TIENDAS PERUANAS S.A.",
            ReceivedBy: null,
            Comments: null,
            Evidences: evidences ?? Array.Empty<EvidenceMessage>(),
            EventDate: DateTime.UtcNow);
    }

    private static TmsEvent ToDomainEvent(TmsEventMessage message)
    {
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
            Array.Empty<Evidence>(),
            message.EventDate);
    }
}