using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IV.DX.Hosting
{
    internal sealed class DXEncryptionRotationService(
        IDXEncryptionKeyProvider keyProvider,
        IOptions<DXEncryptionOptions> encryptionOptions,
        IServiceProvider serviceProvider,
        ILogger<DXEncryptionRotationService> logger) : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Only applies to the default provider — custom providers manage rotation themselves.
            if (keyProvider is not DXConfiguredEncryptionKeyProvider)
            {
                logger.LogDebug("Custom IDXEncryptionKeyProvider detected. Skipping automatic encryption key rotation.");
                return Task.CompletedTask;
            }

            var currentKey = encryptionOptions.Value.Key?.Trim();

            // KeyId is read from the provider — it either came from config or was derived
            // from the key bytes, so this is always the correct id for the current key.
            var currentKeyId = keyProvider.GetCurrent().KeyId;

            if (string.IsNullOrWhiteSpace(currentKey))
                return Task.CompletedTask;

            var state = DXConfiguredEncryptionKeyProvider.ReadState();

            if (state == null)
            {
                // First startup — record the current key so future rotations can detect a change.
                DXConfiguredEncryptionKeyProvider.WriteState(currentKey, currentKeyId);
                logger.LogInformation("Encryption key state file initialized.");
                return Task.CompletedTask;
            }

            if (string.Equals(state.Key, currentKey, StringComparison.Ordinal))
            {
                logger.LogDebug("Encryption key unchanged. No rotation needed.");
                return Task.CompletedTask;
            }

            // Key has changed — run migration in the background so startup is not blocked.
            logger.LogInformation(
                "Encryption key rotation detected. Starting background re-encryption migration.");

            _ = Task.Run(() => RunMigrationAsync(currentKey, currentKeyId, cancellationToken), cancellationToken);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private async Task RunMigrationAsync(string newKey, string newKeyId, CancellationToken ct)
        {
            try
            {
                await using var scope = serviceProvider.CreateAsyncScope();
                var migrationService = scope.ServiceProvider.GetRequiredService<IDXEncryptionMigrationService>();

                var result = await migrationService.MigrateAsync(ct);

                if (result.IsComplete)
                {
                    DXConfiguredEncryptionKeyProvider.WriteState(newKey, newKeyId);
                    logger.LogInformation(
                        "Encryption key rotation complete. {Reencrypted} record(s) re-encrypted.",
                        result.Reencrypted);
                }
                else
                {
                    logger.LogError(
                        "Encryption key rotation finished with errors. {Reencrypted} succeeded, {Failed} failed. " +
                        "State file was NOT updated — the previous key remains available for decryption. " +
                        "Resolve the failures and restart to retry rotation.",
                        result.Reencrypted, result.Failed);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Encryption key rotation migration threw an unexpected exception. " +
                    "The previous key remains available for decryption. Restart to retry.");
            }
        }
    }
}
