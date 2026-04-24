namespace IV.DX.Persistence.Contracts.Abstractions
{
    public interface IDXExecutionContextAccessor
    {
        DXExecutionContext? Current { get; }
        IDisposable BeginScope(DXExecutionContext context);
    }
}

