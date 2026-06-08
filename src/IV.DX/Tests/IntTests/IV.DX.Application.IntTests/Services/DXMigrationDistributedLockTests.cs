using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Hosting;
using IV.DX.Kernel.Attributes;
using IV.DX.Persistence.Contracts.Abstractions;
using IV.DX.Shared.IntTests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace IV.DX.Application.IntTests.Services
{
    [Collection("DX:one-time")]
    public class DXMigrationDistributedLockTests : IntTestController
    {
        private readonly DXTestFixture _fx;

        public DXMigrationDistributedLockTests(DXTestFixture fx, ITestOutputHelper output)
            : base(fx, output)
        {
            _fx = fx;
        }

        [Fact]
        public async Task MigrateCustomEmbeddedAsync_WhenLockIsHeld_ThrowsTimeout()
        {
            var lockName = $"IV.DX.IntTests.MigrationLock.{Guid.NewGuid():N}";
            using var lockHolderRoot = BuildIsolatedRoot(_fx.ConnectionString, lockTimeoutSeconds: 30, lockName);
            using var contenderRoot = BuildIsolatedRoot(_fx.ConnectionString, lockTimeoutSeconds: 1, lockName);

            using var lockHolderScope = lockHolderRoot.CreateScope();
            var distributedLock = lockHolderScope.ServiceProvider.GetRequiredService<IDXMigrationDistributedLock>();
            await using var lockLease = await distributedLock.AcquireAsync();

            using var contenderScope = contenderRoot.CreateScope();
            var migrationService = contenderScope.ServiceProvider.GetRequiredService<IDXMigrationService>();
            var scriptsAssembly = Assembly.GetAssembly(typeof(DXUnitAttribute));

            var exception = await Assert.ThrowsAsync<TimeoutException>(() =>
                migrationService.MigrateCustomEmbeddedAsync(scriptsAssembly, "Data/DXQuery.json"));

            Assert.Contains("migration lock", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task MigrateCustomEmbeddedAsync_WhenLockReleased_Completes()
        {
            var lockName = $"IV.DX.IntTests.MigrationLock.{Guid.NewGuid():N}";
            using var lockHolderRoot = BuildIsolatedRoot(_fx.ConnectionString, lockTimeoutSeconds: 30, lockName);
            using var contenderRoot = BuildIsolatedRoot(_fx.ConnectionString, lockTimeoutSeconds: 3, lockName);

            using (var lockHolderScope = lockHolderRoot.CreateScope())
            {
                var distributedLock = lockHolderScope.ServiceProvider.GetRequiredService<IDXMigrationDistributedLock>();
                await using var lockLease = await distributedLock.AcquireAsync();
            }

            using var contenderScope = contenderRoot.CreateScope();
            var migrationService = contenderScope.ServiceProvider.GetRequiredService<IDXMigrationService>();
            var scriptsAssembly = Assembly.GetAssembly(typeof(DXUnitAttribute));

            await migrationService.MigrateCustomEmbeddedAsync(scriptsAssembly, "Migration/DXQuery.json");
        }

        private static ServiceProvider BuildIsolatedRoot(string connectionString, int lockTimeoutSeconds, string lockName)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "Secrets:DatabaseConnectionString", connectionString },
                    { "Secrets:DatabaseType", "PostgreSQL" },
                    { "Secrets:JwtSigningKey", "int-tests-signing-key-change-me-32-bytes" },
                    { "Secrets:EncryptionKey", "dGVzdC1lbmNyeXB0aW9uLWtleS0zMi1ieXRlcy0hISE=" },
                    { "Database:MigrationLockEnabled", "true" },
                    { "Database:MigrationLockTimeoutSeconds", lockTimeoutSeconds.ToString() },
                    { "Database:MigrationLockPollIntervalMilliseconds", "100" },
                    { "Database:MigrationLockName", lockName }
                })
                .AddEnvironmentVariables()
                .Build();

            var services = new ServiceCollection();
            services.AddDX(configuration).Build();

            var root = services.BuildServiceProvider();

            return root;
        }
    }
}
