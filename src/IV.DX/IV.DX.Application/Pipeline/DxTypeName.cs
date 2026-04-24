namespace IV.DX.Application.Pipeline
{
    internal static class DXTypeName
    {
        public static string Get(Type t)
            => t.GetCustomAttributes(typeof(DXNameAttribute), false) is { Length: > 0 } a
               ? ((DXNameAttribute)a[0]).Name
               : t.Name; // fallback
    }

}
