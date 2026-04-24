namespace IV.DX.Persistence.Contracts.Abstractions
{
    internal interface IDXMigrationDistributedLock
    {
        Task<IAsyncDisposable> AcquireAsync(CancellationToken ct = default);
    }
}
