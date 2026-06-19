using IntegrationServices.Domain.Orders;

namespace IntegrationServices.Application.Events;

public sealed record ProcessTmsEventResult(
    string OrderNumber,
    OrderStatus Status,
    bool WasProcessed,
    bool WasDuplicated,
    bool WasIgnoredBecauseFinalState,
    bool ToBeReturnWasEmitted
);