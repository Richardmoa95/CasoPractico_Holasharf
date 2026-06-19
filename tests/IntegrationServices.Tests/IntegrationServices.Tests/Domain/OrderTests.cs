using FluentAssertions;
using IntegrationServices.Domain.Common;
using IntegrationServices.Domain.Orders;
using IntegrationServices.Domain.ValueObjects;

namespace IntegrationServices.Tests.Domain;

public sealed class OrderTests
{
    [Fact]
    public void ApplyTmsEvent_Should_Update_Order_Status()
    {
        var order = CreateOrder();

        var tmsEvent = CreateTmsEvent(OrderStatus.Started);

        order.ApplyTmsEvent(tmsEvent);

        order.Status.Should().Be(OrderStatus.Started);
    }

    [Fact]
    public void ApplyTmsEvent_Should_Increase_VisitCounter_When_Status_Is_Delivered()
    {
        var order = CreateOrder();

        var tmsEvent = CreateTmsEvent(OrderStatus.Delivered);

        order.ApplyTmsEvent(tmsEvent);

        order.VisitCounter.Value.Should().Be(1);
    }

    [Fact]
    public void ApplyTmsEvent_Should_Increase_VisitCounter_When_Status_Is_NotDelivered()
    {
        var order = CreateOrder();

        var tmsEvent = CreateTmsEvent(OrderStatus.NotDelivered);

        order.ApplyTmsEvent(tmsEvent);

        order.VisitCounter.Value.Should().Be(1);
    }

    [Fact]
    public void ApplyTmsEvent_Should_Not_Increase_VisitCounter_For_Collected()
    {
        var order = CreateOrder();

        var tmsEvent = CreateTmsEvent(OrderStatus.Collected);

        order.ApplyTmsEvent(tmsEvent);

        order.VisitCounter.Value.Should().Be(0);
    }

    [Fact]
    public void ApplyTmsEvent_Should_Throw_When_Order_Is_In_Final_State()
    {
        var order = CreateOrder();

        order.ApplyTmsEvent(CreateTmsEvent(OrderStatus.Delivered));

        var act = () => order.ApplyTmsEvent(CreateTmsEvent(OrderStatus.Started));

        act.Should().Throw<DomainException>()
            .WithMessage($"Order {order.OrderNumber} is already in final state Delivered.");
    }

    [Fact]
    public void MarkToBeReturned_Should_Update_Status_To_ToBeReturn()
    {
        var order = CreateOrder();

        order.MarkToBeReturned();

        order.Status.Should().Be(OrderStatus.ToBeReturn);
    }

    private static Order CreateOrder()
    {
        return Order.Create(
            orderNumber: "2500000006-01",
            trackingNumber: "OE2500000006-01",
            clientCode: "01021755",
            clientName: "TIENDAS PERUANAS S.A.");
    }

    private static TmsEvent CreateTmsEvent(OrderStatus status)
    {
        return new TmsEvent(
            serviceType: ServiceType.LastMile,
            dispatchType: DispatchType.HomeDelivery,
            status: status,
            subStatus: null,
            vehicleCode: "LIMURB06VAN",
            courierName: "Conductor 46",
            orderNumber: "2500000006-01",
            trackingNumber: "OE2500000006-01",
            clientCode: "01021755",
            clientName: "TIENDAS PERUANAS S.A.",
            receivedBy: null,
            comments: null,
            evidences: Array.Empty<Evidence>(),
            eventDate: DateTime.UtcNow);
    }
}