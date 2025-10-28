using IV.DX.Kernel.Converters.JObjectConverters;
using IV.DX.Kernel.Models;

namespace IV.DX.Kernel.Converters.DXObjectConverters
{
    internal static class DXElementConverter
    {
        public static T? ToDXElement<T>(this DXSingleElement? item) where T : DXElement
        {
            if (item is null) return null;

            var jProp = item.ConvertToJPropertyWithoutSystemProperties();
            return (T?)jProp?.Value?.ToObject(typeof(T));
        }
    }
}
