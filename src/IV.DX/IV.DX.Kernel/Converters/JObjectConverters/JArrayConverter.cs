using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Converters.JObjectConverters
{
    internal static class JArrayConverter
    {
        public static JArray ToJArray(this IEnumerable<DXItem> dxItems, bool exlcudeSystemProperties = false)
        {
            var jArray = new JArray();

            foreach (var dxItem in dxItems)
            {
                jArray.Add(dxItem.ToJObject(exlcudeSystemProperties));
            }

            return jArray;
        }
    }
}