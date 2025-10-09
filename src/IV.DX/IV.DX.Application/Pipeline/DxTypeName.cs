namespace IV.DX.Application.Pipeline
{
    internal static class DxTypeName
    {
        public static string Get(Type t)
            => t.GetCustomAttributes(typeof(DxNameAttribute), false) is { Length: > 0 } a
               ? ((DxNameAttribute)a[0]).Name
               : t.Name; // fallback
    }

}
