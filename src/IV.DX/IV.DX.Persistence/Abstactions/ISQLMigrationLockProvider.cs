using System.Data.Common;

namespace IV.DX.Persistence.Abstractions
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
