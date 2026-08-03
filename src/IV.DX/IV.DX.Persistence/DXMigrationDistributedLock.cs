using IV.DX.Persistence.Contracts.Abstractions;
using Microsoft.Extensions.Options;
using System.Data.Common;
using System.Diagnostics;

namespace IV.DX.Persistence
{
    internal sealed class DXMigrationDistributedLock(
        ISQLDbProvider dbProvider,
        ISQLMigrationLockHelper migrationLockHelper,
        IOptions<DXDatabaseOptions> databaseOptions) : IDXMigrationDistributedLock
    {
        private readonly ISQLDbProvider _dbProvider = dbProvider;
        private readonly ISQLMigrationLockHelper _migrationLockHelper = migrationLockHelper;
        private readonly DXDatabaseOptions _databaseOptions = databaseOptions?.Value ?? new DXDatabaseOptions();

        public async Task<IAsyncDisposable> AcquireAsync(CancellationToken ct = default)
        {
            if (!_databaseOptions.MigrationLockEnabled)
            {
                return NoopLease.Instance;
            }

            if (string.IsNullOrWhiteSpace(_databaseOptions.ConnectionString))
            {
                throw new InvalidOperationException("Database:ConnectionString is required for migration lock.");
            }

            var connection = _dbProvider.GetDBConnection(_databaseOptions.ConnectionString);

            try
            {
                await connection.OpenAsync(ct).ConfigureAwait(false);

                var lockName = ResolveLockName();
                await AcquireWithRetryAsync(connection, lockName, ct).ConfigureAwait(false);
                return new DbConnectionLease(connection, _migrationLockHelper, lockName);
            }
            catch
            {
                await connection.DisposeAsync().ConfigureAwait(false);
                throw;
            }
        }

        private async Task AcquireWithRetryAsync(DbConnection connection, string lockName, CancellationToken ct)
        {
            var timeout = ResolveTimeout();
            var pollInterval = ResolvePollInterval();
            var started = Stopwatch.StartNew();

            while (true)
            {
                ct.ThrowIfCancellationRequested();

                var acquired = await _migrationLockHelper
                    .TryAcquireMigrationLockAsync(connection, lockName, ct)
                    .ConfigureAwait(false);

                if (acquired)
                {
                    return;
                }

                if (started.Elapsed >= timeout)
                {
                    throw new TimeoutException($"Timed out while waiting for migration lock '{lockName}'.");
                }

                await Task.Delay(pollInterval, ct).ConfigureAwait(false);
            }
        }

        private int ResolveTimeoutSeconds()
        {
            return _databaseOptions.MigrationLockTimeoutSeconds <= 0
                ? 30
                : _databaseOptions.MigrationLockTimeoutSeconds;
        }

        private TimeSpan ResolveTimeout()
        {
            return TimeSpan.FromSeconds(ResolveTimeoutSeconds());
        }

        private TimeSpan ResolvePollInterval()
        {
            var pollMilliseconds = _databaseOptions.MigrationLockPollIntervalMilliseconds <= 0
                ? 250
                : _databaseOptions.MigrationLockPollIntervalMilliseconds;

            return TimeSpan.FromMilliseconds(pollMilliseconds);
        }

        private string ResolveLockName()
        {
            return string.IsNullOrWhiteSpace(_databaseOptions.MigrationLockName)
                ? "IV.DX.Migrations"
                : _databaseOptions.MigrationLockName.Trim();
        }

        private sealed class DbConnectionLease : IAsyncDisposable
        {
            private readonly ISQLMigrationLockHelper _migrationLockHelper;
            private readonly string _lockName;
            private DbConnection? _connection;

            public DbConnectionLease(
                DbConnection connection,
                ISQLMigrationLockHelper migrationLockHelper,
                string lockName)
            {
                _connection = connection;
                _migrationLockHelper = migrationLockHelper;
                _lockName = lockName;
            }

            public async ValueTask DisposeAsync()
            {
                var activeConnection = Interlocked.Exchange(ref _connection, null);
                if (activeConnection == null)
                {
                    return;
                }

                try
                {
                    await _migrationLockHelper
                        .ReleaseMigrationLockAsync(activeConnection, _lockName, CancellationToken.None)
                        .ConfigureAwait(false);
                }
                catch
                {
                    // Lock should still be released when the session is disposed.
                }
                finally
                {
                    await activeConnection.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        private sealed class NoopLease : IAsyncDisposable
        {
            public static readonly NoopLease Instance = new();

            public ValueTask DisposeAsync()
            {
                return ValueTask.CompletedTask;
            }
        }
    }
}
