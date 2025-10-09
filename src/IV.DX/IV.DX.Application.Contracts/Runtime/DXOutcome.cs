namespace IV.DX.Application.Contracts.Runtime
{
    public enum DXOutcome
    {
        None,
        FromCache,         
        NoOp,
        AlreadyExists,
        AlreadyUpToDate,
        SoftDeleted,
        CreatedViaUpsert
    }
}