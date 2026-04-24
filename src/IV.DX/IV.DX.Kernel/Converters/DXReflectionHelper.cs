using IV.DX.Kernel.Helpers;
using System.Reflection;

namespace IV.DX.Kernel.Converters
{
    internal static class DXReflectionHelper
    {
        public static IEnumerable<PropertyInfo> GetPropsWithAttribute<TAttr>(Type type) where TAttr : Attribute =>
            type.GetProperties().Where(p => AttributeReader.GetAttribute<TAttr>(p) != null);

        public static TAttr? GetAttr<TAttr>(MemberInfo member) where TAttr : Attribute =>
            AttributeReader.GetAttribute<TAttr>(member);

        public static TAttr? GetAttr<TAttr>(Type type) where TAttr : Attribute =>
            AttributeReader.GetAttribute<TAttr>(type);
    }
}
