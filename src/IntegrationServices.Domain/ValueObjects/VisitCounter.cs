using IntegrationServices.Domain.Common;

namespace IntegrationServices.Domain.ValueObjects;

public sealed record VisitCounter
{
    public int Value { get; }

    private VisitCounter(int value)
    {
        if (value < 0)
        {
            throw new DomainException("Visit counter cannot be negative.");
        }

        Value = value;
    }

    public static VisitCounter Zero() => new(0);

    public static VisitCounter From(int value) => new(value);

    public VisitCounter Increment()
    {
        return new VisitCounter(Value + 1);
    }

    public bool HasReachedReturnLimit()
    {
        return Value >= 3;
    }
}