using IV.DX.Kernel.Helpers;
using System.Security.Cryptography;
using Xunit;

namespace IV.DX.Kernel.UnitTests.Encryption
{
    public class DXAesGcmStringProtectorTests
    {
        // ──────────────────────────────────────────────────────────────────────
        // Helpers
        // ──────────────────────────────────────────────────────────────────────

        private static byte[] NewKey() => RandomNumberGenerator.GetBytes(32);

        private static DXAesGcmStringProtector Protector(byte[] keyBytes, string keyId = "k1")
        {
            var key = new DXEncryptionKey(keyId, keyBytes);
            return new DXAesGcmStringProtector(new SingleKeyProvider(key));
        }

        private static DXAesGcmStringProtector MultiKeyProtector(
            byte[] currentKeyBytes, string currentKeyId,
            byte[] previousKeyBytes, string previousKeyId)
        {
            var current = new DXEncryptionKey(currentKeyId, currentKeyBytes);
            var previous = new DXEncryptionKey(previousKeyId, previousKeyBytes);
            return new DXAesGcmStringProtector(new TwoKeyProvider(current, previous));
        }

        // ──────────────────────────────────────────────────────────────────────
        // Format
        // ──────────────────────────────────────────────────────────────────────

        [Fact]
        public void Protect_ProducesExpectedFormat()
        {
            var result = Protector(NewKey()).Protect("hello");

            Assert.StartsWith("$aesgcm$v=1$kid=k1$", result);
        }

        [Fact]
        public void Protect_TwiceSamePlaintext_ProducesDifferentCiphertexts()
        {
            // nonce is random — same plaintext must never produce the same ciphertext
            var protector = Protector(NewKey());
            var c1 = protector.Protect("hello");
            var c2 = protector.Protect("hello");

            Assert.NotEqual(c1, c2);
        }

        // ──────────────────────────────────────────────────────────────────────
        // Round-trip
        // ──────────────────────────────────────────────────────────────────────

        [Theory]
        [InlineData("hello")]
        [InlineData("")]
        [InlineData("unicode: 日本語 🔑")]
        [InlineData("a very long string that is definitely longer than one AES block padding boundary situation")]
        public void Protect_Unprotect_RoundTrip(string plaintext)
        {
            var protector = Protector(NewKey());
            Assert.Equal(plaintext, protector.Unprotect(protector.Protect(plaintext)));
        }

        // ──────────────────────────────────────────────────────────────────────
        // Decryption with previous key (rotation scenario)
        // ──────────────────────────────────────────────────────────────────────

        [Fact]
        public void Unprotect_CiphertextFromPreviousKey_Succeeds()
        {
            // Encrypt with key-v1
            var key1 = NewKey();
            var ciphertext = Protector(key1, "key-v1").Protect("secret");

            // "Rotate" — new protector knows both key-v2 (current) and key-v1 (previous)
            var key2 = NewKey();
            var protectorAfterRotation = MultiKeyProtector(key2, "key-v2", key1, "key-v1");

            Assert.Equal("secret", protectorAfterRotation.Unprotect(ciphertext));
        }

        [Fact]
        public void Protect_AfterRotation_UsesNewKey()
        {
            var key1 = NewKey();
            var key2 = NewKey();
            var protector = MultiKeyProtector(key2, "key-v2", key1, "key-v1");

            var ciphertext = protector.Protect("new data");

            Assert.Contains("kid=key-v2", ciphertext);
        }

        // ──────────────────────────────────────────────────────────────────────
        // Wrong key / missing key
        // ──────────────────────────────────────────────────────────────────────

        [Fact]
        public void TryUnprotect_WithWrongKey_ReturnsFalse()
        {
            var ciphertext = Protector(NewKey(), "k1").Protect("secret");
            var result = Protector(NewKey(), "k1").TryUnprotect(ciphertext, out var plaintext);

            Assert.False(result);
            Assert.Equal(string.Empty, plaintext);
        }

        [Fact]
        public void TryUnprotect_KeyIdNotKnownToProvider_ReturnsFalse()
        {
            var key1 = NewKey();
            var ciphertext = Protector(key1, "key-v1").Protect("secret");

            // New protector only knows "key-v2" — doesn't have "key-v1"
            var result = Protector(NewKey(), "key-v2").TryUnprotect(ciphertext, out _);

            Assert.False(result);
        }

        [Fact]
        public void Unprotect_WithWrongKey_Throws()
        {
            var ciphertext = Protector(NewKey(), "k1").Protect("secret");
            Assert.Throws<System.Security.Cryptography.CryptographicException>(
                () => Protector(NewKey(), "k1").Unprotect(ciphertext));
        }

        // ──────────────────────────────────────────────────────────────────────
        // Invalid inputs
        // ──────────────────────────────────────────────────────────────────────

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("not-encrypted")]
        [InlineData("$aesgcm$corrupted")]
        [InlineData("$aesgcm$v=1$kid=k1$onlythreeparts")]
        [InlineData("$aesgcm$v=2$kid=k1$a$b$c$d")]
        public void TryUnprotect_InvalidInput_ReturnsFalse(string input)
        {
            Assert.False(Protector(NewKey()).TryUnprotect(input, out _));
        }

        [Fact]
        public void Protect_NullInput_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => Protector(NewKey()).Protect(null!));
        }

        // ──────────────────────────────────────────────────────────────────────
        // Private test providers
        // ──────────────────────────────────────────────────────────────────────

        private sealed class SingleKeyProvider(DXEncryptionKey key) : IDXEncryptionKeyProvider
        {
            public DXEncryptionKey GetCurrent() => key;

            public bool TryGet(string keyId, out DXEncryptionKey result)
            {
                if (keyId == key.KeyId) { result = key; return true; }
                result = null!;
                return false;
            }
        }

        private sealed class TwoKeyProvider(DXEncryptionKey current, DXEncryptionKey previous)
            : IDXEncryptionKeyProvider
        {
            public DXEncryptionKey GetCurrent() => current;

            public bool TryGet(string keyId, out DXEncryptionKey result)
            {
                if (keyId == current.KeyId) { result = current; return true; }
                if (keyId == previous.KeyId) { result = previous; return true; }
                result = null!;
                return false;
            }
        }
    }
}
