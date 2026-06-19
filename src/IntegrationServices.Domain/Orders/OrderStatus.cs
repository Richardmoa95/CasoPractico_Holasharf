namespace IntegrationServices.Domain.Orders;

public enum OrderStatus
{
    Planning,
    Started,
    AtPickupPoint,
    Collected,
    NotCollected,
    Delivered,
    NotDelivered,
    ToBeReturn,
    Returned,
    NotReturned
}