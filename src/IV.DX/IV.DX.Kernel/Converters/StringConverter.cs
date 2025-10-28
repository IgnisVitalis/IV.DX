using IV.DX.Kernel.Converters.JObjectConverters;
using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Converters
{
    internal static class StringConverter
    {
        public static string ConvertToString(this DXUnit dxUnit) => JObjectConverter.ToJObject(dxUnit).ToString();

        public static string? ConvertToString(this IEnumerable<DXUnit>? objects)
        {
            if (objects is null) return null;
            var array = new JArray(objects.Select(o => JObjectConverter.ToJObject(o)));
            return array.ToString();
        }
    }
}