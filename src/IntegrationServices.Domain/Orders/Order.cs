using IntegrationServices.Domain.Common;
using IntegrationServices.Domain.ValueObjects;

namespace IntegrationServices.Domain.Orders;

public sealed class Order : EntityBase<string>
{
    private readonly List<Evidence> _evidences = new();

    public string OrderNumber => Id;
    public string TrackingNumber { get; private set; }
    public string ClientCode { get; private set; }
    public string ClientName { get; private set; }
    public OrderStatus Status { get; private set; }
    public VisitCounter VisitCounter { get; private set; }
    public DateTime LastUpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<Evidence> Evidences => _evidences.AsReadOnly();

    private Order(string orderNumber, string trackingNumber, string clientCode, string clientName, OrderStatus status, VisitCounter visitCounter) : base(orderNumber)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
        {
            throw new DomainException("Order number is required.");
        }

        if (string.IsNullOrWhiteSpace(trackingNumber))
        {
            throw new DomainException("Tracking number is required.");
        }

        if (string.IsNullOrWhiteSpace(clientCode))
        {
            throw new DomainException("Client code is required.");
        }

        if (string.IsNullOrWhiteSpace(clientName))
        {
            throw new DomainException("Client name is required.");
        }

        TrackingNumber = trackingNumber;
        ClientCode = clientCode;
        ClientName = clientName;
        Status = status;
        VisitCounter = visitCounter;
        LastUpdatedAtUtc = DateTime.UtcNow;
    }

    public static Order Create(string orderNumber, string trackingNumber, string clientCode, string clientName) 
    {
        return new Order(orderNumber, trackingNumber, clientCode, clientName, OrderStatus.Planning, VisitCounter.Zero());
    }

    public bool IsFinalState()
    {
        return Status is OrderStatus.Delivered or OrderStatus.Returned;
    }

    public bool CanBeUpdated()
    {
        return !IsFinalState();
    }

    public bool ShouldBeReturned()
    {
        return VisitCounter.HasReachedReturnLimit()
            && Status is not OrderStatus.Returned
            && Status is not OrderStatus.Delivered;
    }

    public void ApplyTmsEvent(TmsEvent tmsEvent)
    {
        if (tmsEvent.OrderNumber != OrderNumber)
        {
            throw new DomainException("TMS event does not belong to this order.");
        }

        if (IsFinalState())
        {
            throw new DomainException($"Order {OrderNumber} is already in final state {Status}.");
        }

        Status = tmsEvent.Status;

        if (tmsEvent.ShouldIncreaseVisitCounter())
        {
            VisitCounter = VisitCounter.Increment();
        }

        LastUpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkToBeReturned()
    {
        if (IsFinalState())
        {
            throw new DomainException($"Order {OrderNumber} is already in final state {Status}.");
        }

        Status = OrderStatus.ToBeReturn;
        LastUpdatedAtUtc = DateTime.UtcNow;
    }

    public void AddEvidence(Evidence evidence)
    {
        _evidences.Add(evidence);
        LastUpdatedAtUtc = DateTime.UtcNow;
    }

    public void AddEvidences(IEnumerable<Evidence> evidences)
    {
        foreach (var evidence in evidences)
        {
            AddEvidence(evidence);
        }
    }
}