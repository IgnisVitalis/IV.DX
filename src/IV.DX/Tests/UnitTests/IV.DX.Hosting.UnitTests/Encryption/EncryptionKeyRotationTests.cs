using IV.DX.Hosting;
using IV.DX.Kernel.Helpers;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using Xunit;

namespace IV.DX.Hosting.UnitTests.Encryption
{
    /// <summary>
    /// End-to-end tests for the full encryption key rotation flow:
    /// encrypt → rotate → old data still decrypts → new data uses new key → remove old key → old data fails.
    /// </summary>
    [Collection("EncryptionState")]
    public sealed class EncryptionKeyRotationTests : IDisposable
    {
        private readonly string _stateFile;

        public EncryptionKeyRotationTests()
        {
            _stateFile = Path.Combine(Path.GetTempPath(), $"dx-rot-{Guid.NewGuid()}.json");
            DXConfiguredEncryptionKeyProvider.StateFilePath = _stateFile;
        }

        public void Dispose()
        {
            try { if (File.Exists(_stateFile)) File.Delete(_stateFile); }
            catch (IOException) { /* best-effort cleanup of temp file */ }
        }

        private static byte[] NewKey() => RandomNumberGenerator.GetBytes(32);

        private static DXAesGcmStringProtector ProtectorFor(DXConfiguredEncryptionKeyProvider provider)
            => new(provider);

        private static DXConfiguredEncryptionKeyProvider ProviderFor(byte[] keyBytes)
        {
            var opts = Options.Create(new DXEncryptionOptions { Key = Convert.ToBase64String(keyBytes) });
            return new DXConfiguredEncryptionKeyProvider(opts);
        }

        // ──────────────────────────────────────────────────────────────────────
        // Core rotation scenario
        // ──────────────────────────────────────────────────────────────────────

        [Fact]
        public void AfterRotation_OldCiphertext_StillDecrypts()
        {
            // --- Phase 1: initial state, encrypt some data with key-v1 ---
            var key1 = NewKey();
            var providerV1 = ProviderFor(key1);
            var ciphertext = ProtectorFor(providerV1).Protect("sensitive data");

            // Simulate state file being written on first startup
            DXConfiguredEncryptionKeyProvider.WriteState(
                Convert.ToBase64String(key1),
                providerV1.GetCurrent().KeyId);

            // --- Phase 2: operator sets new key, app restarts ---
            var key2 = NewKey();
            var providerV2 = ProviderFor(key2);  // reads state file → loads key1 as previous

            var protectorV2 = ProtectorFor(providerV2);

            // Old ciphertext (kid=<id of key1>) must still decrypt
            Assert.Equal("sensitive data", protectorV2.Unprotect(ciphertext));
        }

        [Fact]
        public void AfterRotation_NewCiphertext_UsesNewKey()
        {
            var key1 = NewKey();
            var key1Id = DXConfiguredEncryptionKeyProvider.DeriveKeyId(key1);

            DXConfiguredEncryptionKeyProvider.WriteState(Convert.ToBase64String(key1), key1Id);

            var key2 = NewKey();
            var key2Id = DXConfiguredEncryptionKeyProvider.DeriveKeyId(key2);
            var providerV2 = ProviderFor(key2);

            var newCiphertext = ProtectorFor(providerV2).Protect("new data");

            Assert.Contains($"kid={key2Id}", newCiphertext);
            Assert.DoesNotContain($"kid={key1Id}", newCiphertext);
        }

        [Fact]
        public void AfterRotation_ReencryptedData_DecryptsWithNewKey()
        {
            // Simulate re-encryption: decrypt with provider-v2 (which knows both keys),
            // then re-encrypt — result should use key-v2 and be decryptable by key-v2 alone.
            var key1 = NewKey();
            DXConfiguredEncryptionKeyProvider.WriteState(
                Convert.ToBase64String(key1),
                DXConfiguredEncryptionKeyProvider.DeriveKeyId(key1));

            var key2 = NewKey();
            var providerV2 = ProviderFor(key2);
            var protectorV2 = ProtectorFor(providerV2);

            var originalCiphertext = ProtectorFor(ProviderFor(key1)).Protect("secret");

            // Re-encrypt (what DXEncryptionMigrationService does per record)
            var plaintext = protectorV2.Unprotect(originalCiphertext);
            var reencrypted = protectorV2.Protect(plaintext);

            // After rotation completes and state file is updated, key1 is removed.
            // The re-encrypted value must decrypt with only key2.
            var providerV2Only = ProviderFor(key2); // fresh provider, no state file (key matches)
            DXConfiguredEncryptionKeyProvider.WriteState(
                Convert.ToBase64String(key2),
                DXConfiguredEncryptionKeyProvider.DeriveKeyId(key2));

            Assert.Equal("secret", ProtectorFor(providerV2Only).Unprotect(reencrypted));
        }

        [Fact]
        public void AfterStateFileUpdated_OldCiphertext_FailsToDecrypt()
        {
            // Once migration is complete the state file is updated to the new key.
            // A fresh restart loads only the new key — old (not-yet-migrated) ciphertexts
            // will fail. This is the expected behaviour signalling migration is complete.
            var key1 = NewKey();
            var ciphertextWithKey1 = ProtectorFor(ProviderFor(key1)).Protect("secret");

            // State file updated to key2 after successful migration
            var key2 = NewKey();
            DXConfiguredEncryptionKeyProvider.WriteState(
                Convert.ToBase64String(key2),
                DXConfiguredEncryptionKeyProvider.DeriveKeyId(key2));

            // Fresh restart — state file key == current env key → no previous key loaded
            var providerAfterMigration = ProviderFor(key2);

            Assert.False(ProtectorFor(providerAfterMigration).TryUnprotect(ciphertextWithKey1, out _));
        }

        // ──────────────────────────────────────────────────────────────────────
        // Multiple rotations
        // ──────────────────────────────────────────────────────────────────────

        [Fact]
        public void RotationTwice_MostRecentPreviousKeyAvailable()
        {
            // key1 → key2 (rotation 1, migration completed) → key3 (rotation 2, migration in progress)
            // During rotation 2: key2 ciphertexts must still decrypt.
            var key2 = NewKey();
            DXConfiguredEncryptionKeyProvider.WriteState(
                Convert.ToBase64String(key2),
                DXConfiguredEncryptionKeyProvider.DeriveKeyId(key2));

            var ciphertextWithKey2 = ProtectorFor(ProviderFor(key2)).Protect("mid-rotation data");

            var key3 = NewKey();
            var providerV3 = ProviderFor(key3); // state file has key2 → loads it as previous

            Assert.Equal("mid-rotation data", ProtectorFor(providerV3).Unprotect(ciphertextWithKey2));
        }
    }
}
