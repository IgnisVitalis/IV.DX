using System.Text.RegularExpressions;

namespace IV.DX.Kernel.Helpers
{
    internal static class StringHelper
    {
        public static IEnumerable<KeyValuePair<string, char?>> SplitAndKeep(this string s, char[] delims)
        {
            int start = 0, index;
            char? delimiter = null;

            while ((index = s.IndexOfAny(delims, start)) != -1)
            {
                if (index - start > 0)
                    yield return new KeyValuePair<string, char?>(s.Substring(start, index - start), delimiter);

                start = index + 1;

                delimiter = s[index];
            }

            if (start < s.Length)
            {
                yield return new KeyValuePair<string, char?>(s.Substring(start), delimiter);
            }
        }

#warning Need to define solution without regex. Regex uses some chars that should be protected.
        public static IEnumerable<KeyValuePair<string, string>> SplitAndKeep(this string s, string[] delims, StringSplitOptions splitOption)
        {
            string pattern = string.Join('|', delims.Select(x => $"({x})"));
            string delimiter = null;

            string[] substrings = Regex.Split(s, pattern);

            for (int i = 0; i < substrings.Length; i = i + 2)
            {
                if (splitOption.HasFlag(StringSplitOptions.RemoveEmptyEntries))
                {
                    if (substrings[i].Trim() == string.Empty)
                    {
                        continue;
                    }
                }

                if (splitOption.HasFlag(StringSplitOptions.TrimEntries))
                {
                    yield return new KeyValuePair<string, string>(substrings[i].Trim(), delimiter);
                }
                else
                {
                    yield return new KeyValuePair<string, string>(substrings[i], delimiter);
                }

                if (i + 1 == substrings.Length)
                    break;

                delimiter = substrings[i + 1];
            }
        }
    }
}