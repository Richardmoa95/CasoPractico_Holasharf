using IntegrationServices.API.Contracts;
using IntegrationServices.Application.Events;
using IntegrationServices.Domain.Orders;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace IntegrationServices.API.Mappers;

public static class TmsWebhookMapper
{
    public static TmsEventMessage ToMessage(TmsWebhookEventRequest request)
    {
        var serviceType = ParseServiceType(request.ServiceType);
        var dispatchType = ParseDispatchType(request.DispatchType);
        var status = ParseOrderStatus(request.Status);
        var eventDate = ParseEventDate(request.EventDate);

        var evidences = request.Details.Evidences?
            .Select(e => new EvidenceMessage(e.Label, e.FileType, e.FileName, e.Url))
            .ToList() ?? new List<EvidenceMessage>();

        var eventId = GenerateEventId(request.Details.OrderNumber, request.Status, request.SubStatus, request.EventDate);

        return new TmsEventMessage(
            EventId: eventId,
            ServiceType: serviceType,
            DispatchType: dispatchType,
            Status: status,
            SubStatus: request.SubStatus,
            VehicleCode: request.VehicleCode,
            CourierName: request.CourierName,
            OrderNumber: request.Details.OrderNumber,
            TrackingNumber: request.Details.TrackingNumber,
            ClientCode: request.Details.ClientCode,
            ClientName: request.Details.ClientName,
            ReceivedBy: request.Details.ReceivedBy,
            Comments: request.Details.Comments,
            Evidences: evidences,
            EventDate: eventDate
        );
    }

    private static ServiceType ParseServiceType(string value)
    {
        return Normalize(value) switch
        {
            "LAST_MILE" => ServiceType.LastMile,
            "PICKUP" => ServiceType.Pickup,
            "RETURN" => ServiceType.Return,
            _ => throw new ArgumentException($"Invalid serviceType: {value}")
        };
    }

    private static DispatchType ParseDispatchType(string value)
    {
        return Normalize(value) switch
        {
            "HOME_DELIVERY" => DispatchType.HomeDelivery,
            "STORE_WITHDRAWAL" => DispatchType.StoreWithdrawal,
            "REVERSE" => DispatchType.Reverse,
            _ => throw new ArgumentException($"Invalid dispatchType: {value}")
        };
    }

    private static OrderStatus ParseOrderStatus(string value)
    {
        return Normalize(value) switch
        {
            "PLANNING" => OrderStatus.Planning,
            "STARTED" => OrderStatus.Started,
            "AT_PICKUP_POINT" => OrderStatus.AtPickupPoint,
            "AT PICKUP POINT" => OrderStatus.AtPickupPoint,
            "COLLECTED" => OrderStatus.Collected,
            "NOT_COLLECTED" => OrderStatus.NotCollected,
            "NOT COLLECTED" => OrderStatus.NotCollected,
            "DELIVERED" => OrderStatus.Delivered,
            "NOT_DELIVERED" => OrderStatus.NotDelivered,
            "NOT DELIVERED" => OrderStatus.NotDelivered,
            "TO_BE_RETURN" => OrderStatus.ToBeReturn,
            "TO BE RETURN" => OrderStatus.ToBeReturn,
            "RETURNED" => OrderStatus.Returned,
            "NOT_RETURNED" => OrderStatus.NotReturned,
            "NOT RETURNED" => OrderStatus.NotReturned,
            _ => throw new ArgumentException($"Invalid status: {value}")
        };
    }

    private static DateTime ParseEventDate(string value)
    {
        var formats = new[]
        {
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-ddTHH:mm:ssZ",
            "O"
        };

        if (DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed))
        {
            return parsed;
        }

        if (DateTime.TryParse(value, out parsed))
        {
            return parsed;
        }

        throw new ArgumentException($"Invalid eventDate: {value}");
    }

    private static string GenerateEventId(string orderNumber, string status, string? subStatus, string eventDate)
    {
        var raw = $"{orderNumber}|{status}|{subStatus}|{eventDate}";

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));

        return Convert.ToHexString(bytes);
    }

    private static string Normalize(string value)
    {
        return value.Trim().ToUpperInvariant();
    }
}