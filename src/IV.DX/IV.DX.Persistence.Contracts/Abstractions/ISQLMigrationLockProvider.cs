using System.Data.Common;

namespace IV.DX.Persistence.Contracts.Abstractions
{
    internal interface ISQLMigrationLockProvider
    {
        Task<IAsyncDisposable> AcquireMigrationLockAsync(
            DbConnection connection,
            string lockName,
            int timeoutSeconds,
            int pollIntervalMilliseconds,
            CancellationToken ct);
    }
}
