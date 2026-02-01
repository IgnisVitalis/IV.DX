using System.Text.RegularExpressions;

namespace IV.DX.Kernel.Helpers
{
    internal static class StringExtensions
    {
        public static IEnumerable<KeyValuePair<string, string>> SplitAndKeep(
            this string s,
            string[] delims,
            StringSplitOptions splitOption)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (delims == null || delims.Length == 0)
                return new[] { new KeyValuePair<string, string>(s, null) };

            string pattern = string.Join('|', delims.Select(x => $"({Regex.Escape(x)})"));
            string delimiter = null;

            string[] substrings = Regex.Split(s, pattern);

            var result = new List<KeyValuePair<string, string>>();

            for (int i = 0; i < substrings.Length; i = i + 2)
            {
                var chunk = substrings[i];

                if (splitOption.HasFlag(StringSplitOptions.RemoveEmptyEntries))
                {
                    if (string.IsNullOrWhiteSpace(chunk))
                        goto advance;
                }

                if (splitOption.HasFlag(StringSplitOptions.TrimEntries))
                    chunk = chunk.Trim();

                result.Add(new KeyValuePair<string, string>(chunk, delimiter));

            advance:
                if (i + 1 == substrings.Length)
                    break;

                delimiter = substrings[i + 1];
            }

            return result;
        }
    }
}
