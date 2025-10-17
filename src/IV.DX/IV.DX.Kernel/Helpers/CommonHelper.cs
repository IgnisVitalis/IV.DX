namespace IV.DX.Kernel.Helpers
{
    internal static class CommonHelper
    {
        public static string ConvertIdsToString(IEnumerable<Guid> ids)
        {
            if (ids == null)
                return null;

            return string.Join(",", ids.Select(x => $"'{x}'"));
        }

        public static string ConvertIdsToString(IEnumerable<string> ids)
        {
            if (ids == null)
                return null;

            return string.Join(",", ids.Select(x => $"'{x}'"));
        }
    }
}
