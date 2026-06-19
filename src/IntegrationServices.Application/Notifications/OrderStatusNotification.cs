using IntegrationServices.Domain.Orders;
using System;
namespace IntegrationServices.Application.Notifications;

public sealed record OrderStatusNotification(
    string OrderNumber,
    string TrackingNumber,
    string ClientCode,
    string ClientName,
    OrderStatus Status,
    string? SubStatus,
    DateTime EventDate
);