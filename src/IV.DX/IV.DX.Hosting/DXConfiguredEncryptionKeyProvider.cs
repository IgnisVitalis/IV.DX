using IV.DX.Kernel.Helpers;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text.Json;

namespace IV.DX.Hosting
{
    internal sealed class DXConfiguredEncryptionKeyProvider : IDXEncryptionKeyProvider
    {
        internal static string StateFilePath { get; set; } =
            Path.Combine(AppContext.BaseDirectory, "encryption-key-state.json");

        private readonly DXEncryptionKey _current;
        private readonly Dictionary<string, DXEncryptionKey> _all;

        public DXConfiguredEncryptionKeyProvider(IOptions<DXEncryptionOptions> options)
        {
            var o = options.Value ?? new DXEncryptionOptions();

            var keyBytes = ResolveCurrentKeyBytes(o);
            var keyId = DeriveKeyId(keyBytes);

            _current = new DXEncryptionKey(keyId, keyBytes);

            _all = new Dictionary<string, DXEncryptionKey>(StringComparer.Ordinal)
            {
                [_current.KeyId] = _current
            };

            // Load previous key from state file for zero-downtime rotation support.
            // The state file records the key and its id, so old encrypted data (which embeds
            // kid=<prevKeyId>) can still be decrypted during migration.
            var state = ReadState();
            if (state != null &&
                !string.IsNullOrWhiteSpace(state.Key) &&
                !string.Equals(state.Key, o.Key?.Trim(), StringComparison.Ordinal))
            {
                var prevBytes = Convert.FromBase64String(state.Key);
                var prevKeyId = string.IsNullOrWhiteSpace(state.KeyId)
                    ? DeriveKeyId(prevBytes)   // backwards-compat: old state files without KeyId
                    : state.KeyId;

                // Don't overwrite the current key if ids happen to collide (shouldn't occur
                // with derived ids but guards against misconfigured explicit ids).
                if (!_all.ContainsKey(prevKeyId))
                    _all[prevKeyId] = new DXEncryptionKey(prevKeyId, prevBytes);
            }
        }

        public DXEncryptionKey GetCurrent() => _current;

        public bool TryGet(string keyId, out DXEncryptionKey key)
        {
            if (!string.IsNullOrWhiteSpace(keyId) && _all.TryGetValue(keyId.Trim(), out var found))
            {
                key = found;
                return true;
            }

            key = null!;
            return false;
        }

        internal static EncryptionKeyState? ReadState()
        {
            if (!File.Exists(StateFilePath)) return null;
            try
            {
                var json = File.ReadAllText(StateFilePath);
                var state = JsonSerializer.Deserialize<EncryptionKeyState>(json);
                return string.IsNullOrWhiteSpace(state?.Key) ? null : state;
            }
            catch { return null; }
        }

        internal static void WriteState(string key, string keyId)
        {
            var state = new EncryptionKeyState { Key = key, KeyId = keyId };
            File.WriteAllText(StateFilePath, JsonSerializer.Serialize(state));
        }

        /// <summary>
        /// Derives a short, stable, URL-safe key Id from the key bytes using SHA-256.
        /// The same key bytes always produce the same Id; different keys produce different IDs.
        /// </summary>
        internal static string DeriveKeyId(byte[] keyBytes)
        {
            var hash = SHA256.HashData(keyBytes);
            return Convert.ToBase64String(hash)[..8]
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static byte[] ResolveCurrentKeyBytes(DXEncryptionOptions o)
        {
            if (string.IsNullOrWhiteSpace(o.Key))
                throw new InvalidOperationException(
                    "Secrets:EncryptionKey is not configured. " +
                    "Provide a Base64-encoded 32-byte AES key via environment variable 'Secrets__EncryptionKey'. " +
                    "Generate one with: openssl rand -base64 32");

            return Convert.FromBase64String(o.Key.Trim());
        }
    }
}
