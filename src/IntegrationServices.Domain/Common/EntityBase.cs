namespace IntegrationServices.Domain.Common;

public abstract class EntityBase<TId>
{
    public TId Id { get; protected set; }

    protected EntityBase(TId id)
    {
        Id = id;
    }
}
