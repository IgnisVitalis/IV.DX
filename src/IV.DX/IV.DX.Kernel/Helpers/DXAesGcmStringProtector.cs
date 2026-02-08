using System.Security.Cryptography;
using System.Text;

namespace IV.DX.Kernel.Helpers
{
    public sealed class DXAesGcmStringProtector(IDXEncryptionKeyProvider keyProvider) : IDXStringProtector
    {
        private const string _prefix = "$aesgcm$";
        private const int _nonceSize = 12;
        private const int _tagSize = 16;

        public string Protect(string plaintext)
        {
            ArgumentNullException.ThrowIfNull(plaintext);

            var key = keyProvider.GetCurrent();
            ValidateKey(key);

            var nonce = RandomNumberGenerator.GetBytes(_nonceSize);
            var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            var ciphertext = new byte[plaintextBytes.Length];
            var tag = new byte[_tagSize];

            using var aes = new AesGcm(key.KeyBytes, tagSizeInBytes: _tagSize);
            aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

            // $aesgcm$v=1$kid=<keyId>$<nonceB64>$<tagB64>$<cipherB64>
            return $"{_prefix}v=1$kid={key.KeyId}${Convert.ToBase64String(nonce)}${Convert.ToBase64String(tag)}${Convert.ToBase64String(ciphertext)}";
        }

        public string Unprotect(string protectedValue)
        {
            if (!TryUnprotect(protectedValue, out var plaintext))
                throw new CryptographicException("Invalid protected value.");
            return plaintext;
        }

        public bool TryUnprotect(string protectedValue, out string plaintext)
        {
            plaintext = string.Empty;
            if (string.IsNullOrWhiteSpace(protectedValue))
                return false;

            if (!protectedValue.StartsWith(_prefix, StringComparison.Ordinal))
                return false;

            // After prefix: v=1$kid=...$nonce$tag$cipher
            var rest = protectedValue.Substring(_prefix.Length);
            var parts = rest.Split('$');
            if (parts.Length != 5)
                return false;

            if (!string.Equals(parts[0], "v=1", StringComparison.Ordinal))
                return false;

            if (!parts[1].StartsWith("kid=", StringComparison.Ordinal))
                return false;

            var keyId = parts[1].Substring("kid=".Length);
            if (string.IsNullOrWhiteSpace(keyId))
                return false;

            if (!keyProvider.TryGet(keyId, out var key))
                return false;

            try
            {
                ValidateKey(key);

                var nonce = Convert.FromBase64String(parts[2]);
                var tag = Convert.FromBase64String(parts[3]);
                var ciphertext = Convert.FromBase64String(parts[4]);

                if (nonce.Length != _nonceSize || tag.Length != _tagSize)
                    return false;

                var plaintextBytes = new byte[ciphertext.Length];
                using var aes = new AesGcm(key.KeyBytes, tagSizeInBytes: _tagSize);
                aes.Decrypt(nonce, ciphertext, tag, plaintextBytes);

                plaintext = Encoding.UTF8.GetString(plaintextBytes);
                return true;
            }
            catch
            {
                plaintext = string.Empty;
                return false;
            }
        }

        private static void ValidateKey(DXEncryptionKey key)
        {
            if (key.KeyBytes == null)
                throw new CryptographicException("Encryption key bytes are null.");

            if (key.KeyBytes.Length != 32)
                throw new CryptographicException("AES-GCM requires a 32-byte key (AES-256) for this implementation.");
        }
    }
}

