using System.Data.Common;

namespace IV.DX.Persistence.Abstractions
{
    internal interface ISQLMigrationLockHelper
    {
        Task<bool> TryAcquireMigrationLockAsync(
            DbConnection connection,
            string lockName,
            CancellationToken cancellationToken);

        Task ReleaseMigrationLockAsync(
            DbConnection connection,
            string lockName,
            CancellationToken cancellationToken);
    }
}
