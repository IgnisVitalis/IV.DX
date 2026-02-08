using IV.DX.Kernel.Helpers;
using Microsoft.Extensions.Options;

namespace IV.DX.Hosting
{
    internal sealed class DXConfiguredEncryptionKeyProvider : IDXEncryptionKeyProvider
    {
        private readonly DXEncryptionKey _current;

        public DXConfiguredEncryptionKeyProvider(IOptions<DXEncryptionOptions> options)
        {
            var o = options.Value ?? new DXEncryptionOptions();

            var keyId = string.IsNullOrWhiteSpace(o.KeyId) ? "v1" : o.KeyId.Trim();
            var keyBytes = ResolveKeyBytes(o);

            _current = new DXEncryptionKey(keyId, keyBytes);
        }

        public DXEncryptionKey GetCurrent() => _current;

        public bool TryGet(string keyId, out DXEncryptionKey key)
        {
            if (!string.IsNullOrWhiteSpace(keyId) && string.Equals(keyId.Trim(), _current.KeyId, StringComparison.Ordinal))
            {
                key = _current;
                return true;
            }

            key = null!;
            return false;
        }

        private static byte[] ResolveKeyBytes(DXEncryptionOptions o)
        {
            var fromConfig = o.KeyBase64;
            if (!string.IsNullOrWhiteSpace(fromConfig))
                return Convert.FromBase64String(fromConfig.Trim());

            var fromFile = o.KeyFile;
            if (!string.IsNullOrWhiteSpace(fromFile) && File.Exists(fromFile))
            {
                var text = File.ReadAllText(fromFile).Trim();
                if (!string.IsNullOrWhiteSpace(text))
                    return Convert.FromBase64String(text);
            }

            // Dev/test fallback key (32 bytes). Override via Encryption:KeyBase64 or Encryption:KeyFile in production.
            return Convert.FromBase64String("AQIDBAUGBwgJCgsMDQ4PEBESExQVFhcYGRobHB0eHyA=");
        }
    }
}

