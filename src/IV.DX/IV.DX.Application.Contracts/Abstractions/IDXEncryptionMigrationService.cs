using IV.DX.Application.Contracts.Models;

namespace IV.DX.Application.Contracts.Abstractions
{
    public interface IDXEncryptionMigrationService
    {
        /// <summary>
        /// Re-encrypts all EncryptedString values using the current key.
        /// Safe to run while the app is live — reads and re-saves each record transparently.
        /// </summary>
        Task<DXEncryptionMigrationResult> MigrateAsync(CancellationToken ct = default);
    }
}
