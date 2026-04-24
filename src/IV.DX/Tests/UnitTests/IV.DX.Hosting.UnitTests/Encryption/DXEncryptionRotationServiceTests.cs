using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.Models;
using IV.DX.Hosting;
using IV.DX.Kernel.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using Xunit;

namespace IV.DX.Hosting.UnitTests.Encryption
{
    [Collection("EncryptionState")]
    public sealed class DXEncryptionRotationServiceTests : IDisposable
    {
        private readonly string _stateFile;

        public DXEncryptionRotationServiceTests()
        {
            _stateFile = Path.Combine(Path.GetTempPath(), $"dx-svc-{Guid.NewGuid()}.json");
            DXConfiguredEncryptionKeyProvider.StateFilePath = _stateFile;
        }

        public void Dispose()
        {
            try { if (File.Exists(_stateFile)) File.Delete(_stateFile); }
            catch (IOException) { /* best-effort: background task may still hold the file */ }
        }

        // ──────────────────────────────────────────────────────────────────────
        // Helpers
        // ──────────────────────────────────────────────────────────────────────

        private static byte[] NewKey() => RandomNumberGenerator.GetBytes(32);
        private static string ToBase64(byte[] b) => Convert.ToBase64String(b);

        private DXEncryptionRotationService CreateService(
            byte[] keyBytes,
            IDXEncryptionMigrationService migrationService,
            IDXEncryptionKeyProvider? keyProviderOverride = null)
        {
            var opts = Options.Create(new DXEncryptionOptions { Key = ToBase64(keyBytes) });
            var keyProvider = keyProviderOverride
                ?? new DXConfiguredEncryptionKeyProvider(opts);

            var services = new ServiceCollection();
            services.AddSingleton(migrationService);
            var sp = services.BuildServiceProvider();

            return new DXEncryptionRotationService(
                keyProvider,
                opts,
                sp,
                NullLogger<DXEncryptionRotationService>.Instance);
        }

        // ──────────────────────────────────────────────────────────────────────
        // First startup — no state file
        // ──────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task StartAsync_NoStateFile_WritesStateFileWithCurrentKey()
        {
            var key = NewKey();
            var svc = CreateService(key, new NoOpMigrationService());

            await svc.StartAsync(CancellationToken.None);

            var state = DXConfiguredEncryptionKeyProvider.ReadState();
            Assert.NotNull(state);
            Assert.Equal(ToBase64(key), state!.Key);
        }

        [Fact]
        public async Task StartAsync_NoStateFile_DoesNotRunMigration()
        {
            var migration = new TrackingMigrationService(success: true);
            var svc = CreateService(NewKey(), migration);

            await svc.StartAsync(CancellationToken.None);

            Assert.False(migration.WasCalled);
        }

        // ──────────────────────────────────────────────────────────────────────
        // Key unchanged
        // ──────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task StartAsync_KeyUnchanged_DoesNotRunMigration()
        {
            var key = NewKey();
            var keyId = DXConfiguredEncryptionKeyProvider.DeriveKeyId(key);
            DXConfiguredEncryptionKeyProvider.WriteState(ToBase64(key), keyId);

            var migration = new TrackingMigrationService(success: true);
            var svc = CreateService(key, migration);

            await svc.StartAsync(CancellationToken.None);

            Assert.False(migration.WasCalled);
        }

        // ──────────────────────────────────────────────────────────────────────
        // Key changed — migration triggered
        // ──────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task StartAsync_KeyChanged_MigrationIsCalled()
        {
            var oldKey = NewKey();
            DXConfiguredEncryptionKeyProvider.WriteState(
                ToBase64(oldKey),
                DXConfiguredEncryptionKeyProvider.DeriveKeyId(oldKey));

            var migration = new TrackingMigrationService(success: true);
            var svc = CreateService(NewKey(), migration);

            await svc.StartAsync(CancellationToken.None);
            await migration.Completed.WaitAsync(TimeSpan.FromSeconds(5));

            Assert.True(migration.WasCalled);
        }

        [Fact]
        public async Task StartAsync_KeyChanged_MigrationSuccess_StateFileUpdatedToNewKey()
        {
            var oldKey = NewKey();
            DXConfiguredEncryptionKeyProvider.WriteState(
                ToBase64(oldKey),
                DXConfiguredEncryptionKeyProvider.DeriveKeyId(oldKey));

            var newKey = NewKey();
            var migration = new TrackingMigrationService(success: true);
            var svc = CreateService(newKey, migration);

            await svc.StartAsync(CancellationToken.None);
            await migration.Completed.WaitAsync(TimeSpan.FromSeconds(5));

            // Brief yield to ensure WriteState runs after MigrateAsync returns
            await Task.Delay(50);

            var state = DXConfiguredEncryptionKeyProvider.ReadState();
            Assert.Equal(ToBase64(newKey), state?.Key);
        }

        [Fact]
        public async Task StartAsync_KeyChanged_MigrationFails_StateFileRetainsOldKey()
        {
            var oldKey = NewKey();
            DXConfiguredEncryptionKeyProvider.WriteState(
                ToBase64(oldKey),
                DXConfiguredEncryptionKeyProvider.DeriveKeyId(oldKey));

            var migration = new TrackingMigrationService(success: false); // partial failure
            var svc = CreateService(NewKey(), migration);

            await svc.StartAsync(CancellationToken.None);
            await migration.Completed.WaitAsync(TimeSpan.FromSeconds(5));
            await Task.Delay(50);

            // State file must still point to the OLD key so previous key stays available
            var state = DXConfiguredEncryptionKeyProvider.ReadState();
            Assert.Equal(ToBase64(oldKey), state?.Key);
        }

        // ──────────────────────────────────────────────────────────────────────
        // Custom provider — rotation is skipped
        // ──────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task StartAsync_CustomProvider_DoesNotRunMigration()
        {
            var migration = new TrackingMigrationService(success: true);
            var svc = CreateService(NewKey(), migration, keyProviderOverride: new CustomProvider());

            await svc.StartAsync(CancellationToken.None);

            Assert.False(migration.WasCalled);
        }

        [Fact]
        public async Task StartAsync_CustomProvider_DoesNotWriteStateFile()
        {
            var svc = CreateService(NewKey(), new NoOpMigrationService(), new CustomProvider());

            await svc.StartAsync(CancellationToken.None);

            Assert.False(File.Exists(_stateFile));
        }

        // ──────────────────────────────────────────────────────────────────────
        // Fake implementations
        // ──────────────────────────────────────────────────────────────────────

        private sealed class NoOpMigrationService : IDXEncryptionMigrationService
        {
            public Task<DXEncryptionMigrationResult> MigrateAsync(CancellationToken ct = default)
                => Task.FromResult(new DXEncryptionMigrationResult { Reencrypted = 0, Failed = 0 });
        }

        private sealed class TrackingMigrationService(bool success) : IDXEncryptionMigrationService
        {
            private readonly TaskCompletionSource _completed = new();

            public bool WasCalled { get; private set; }
            public Task Completed => _completed.Task;

            public Task<DXEncryptionMigrationResult> MigrateAsync(CancellationToken ct = default)
            {
                WasCalled = true;
                _completed.TrySetResult();
                var result = success
                    ? new DXEncryptionMigrationResult { Reencrypted = 5, Failed = 0 }
                    : new DXEncryptionMigrationResult { Reencrypted = 3, Failed = 2 };
                return Task.FromResult(result);
            }
        }

        private sealed class CustomProvider : IDXEncryptionKeyProvider
        {
            private readonly DXEncryptionKey _key =
                new("custom", RandomNumberGenerator.GetBytes(32));

            public DXEncryptionKey GetCurrent() => _key;

            public bool TryGet(string keyId, out DXEncryptionKey key)
            {
                key = _key;
                return keyId == _key.KeyId;
            }
        }
    }
}
