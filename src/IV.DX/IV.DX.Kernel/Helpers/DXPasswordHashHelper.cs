using System.Security.Cryptography;

namespace IV.DX.Kernel.Helpers
{
    public static class DXPasswordHashHelper
    {
        private const string _prefix = "$pbkdf2-sha512$";
        private const int _saltSize = 16;
        private const int _subkeySize = 32;

        public static string Hash(string password, int iterations = 100_000)
        {
            ArgumentNullException.ThrowIfNull(password);
            if (iterations <= 0) throw new ArgumentOutOfRangeException(nameof(iterations));

            var salt = RandomNumberGenerator.GetBytes(_saltSize);
            var subkey = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA512,
                _subkeySize);

            return $"{_prefix}i={iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(subkey)}";
        }

        public static bool Verify(string password, string encodedHash)
        {
            if (password == null || encodedHash == null)
                return false;

            if (!encodedHash.StartsWith(_prefix, StringComparison.Ordinal))
                return false;

            var rest = encodedHash.Substring(_prefix.Length);
            var parts = rest.Split('$');
            if (parts.Length != 3)
                return false;

            if (!parts[0].StartsWith("i=", StringComparison.Ordinal))
                return false;

            if (!int.TryParse(parts[0].Substring(2), out var iterations) || iterations <= 0)
                return false;

            byte[] salt;
            byte[] expected;
            try
            {
                salt = Convert.FromBase64String(parts[1]);
                expected = Convert.FromBase64String(parts[2]);
            }
            catch
            {
                return false;
            }

            var actual = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA512,
                expected.Length);

            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
    }
}
