using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;
using System.Collections.Concurrent;
using System.Reflection;

namespace IV.DX.Kernel.Helpers
{
    internal static class AttributeReader
    {
        // --- Caches ---
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertiesCache = new();
        private static readonly ConcurrentDictionary<(Type type, Type attr), Attribute?> TypeAttrCache = new();
        private static readonly ConcurrentDictionary<(PropertyInfo prop, Type attr), Attribute?> PropAttrCache = new();
        private static readonly ConcurrentDictionary<Type, Type> ElementTypeCache = new();

        // ----- Basic helpers -----
        public static T? GetAttribute<T>(Type type, bool inherit = true) where T : Attribute
        {
            if (type is null) return default;
            var key = (type, typeof(T));
            if (TypeAttrCache.TryGetValue(key, out var cached)) return (T?)cached;
            var attr = type.GetCustomAttribute(typeof(T), inherit) as T;
            TypeAttrCache[key] = attr;
            return attr;
        }

        public static T? GetAttribute<T>(PropertyInfo prop, bool inherit = true) where T : Attribute
        {
            if (prop is null) return default;
            var key = (prop, typeof(T));
            if (PropAttrCache.TryGetValue(key, out var cached)) return (T?)cached;
            var attr = prop.GetCustomAttribute(typeof(T), inherit) as T;
            PropAttrCache[key] = attr;
            return attr;
        }

        public static IEnumerable<T> GetAttributesOnProperties<T>(Type type, bool inherit = true) where T : Attribute
            => GetProperties(type).Select(p => GetAttribute<T>(p, inherit)).Where(a => a is not null)!.Cast<T>();

        // ----- Property discovery -----
        public static IEnumerable<PropertyInfo> GetSingleItemInfos(DXUnit dxUnit)
            => dxUnit is null ? Array.Empty<PropertyInfo>()
                              : GetSingleItemInfos(dxUnit.GetType()).Where(p => p.GetValue(dxUnit) is not null);

        public static IEnumerable<PropertyInfo> GetSingleItemInfos(Type type)
            => GetProperties(type).Where(p =>
                   p.PropertyType.BaseType == typeof(DXElement)
                   && p.GetMethod is not null && !p.GetMethod.IsStatic);

        public static IEnumerable<PropertyInfo> GetMultiItemInfos(DXUnit dxUnit)
            => dxUnit is null ? Array.Empty<PropertyInfo>()
                              : GetMultiItemInfos(dxUnit.GetType()).Where(p => p.GetValue(dxUnit) is not null);

        public static IEnumerable<PropertyInfo> GetMultiItemInfos(Type type)
            => GetProperties(type).Where(p =>
                   p.PropertyType.IsGenericType
                   && p.PropertyType.GetGenericTypeDefinition() == typeof(DXMultiElementsContainer<>)
                   && p.GetMethod is not null && !p.GetMethod.IsStatic);

        private static PropertyInfo[] GetProperties(Type type)
        {
            if (type is null) return Array.Empty<PropertyInfo>();
           
            return PropertiesCache.GetOrAdd(type, t => t.GetProperties());
        }


        public static string GetDXUnitTypeName(Type type)
        {
            var objectType = FindElementType(type);
            var attr = GetAttribute<DXUnitAttribute>(objectType);
            return attr?.Type ?? string.Empty;
        }

        public static string GetDXElementTypeName(Type type)
        {
            var objectType = FindElementType(type);
            var attr = GetAttribute<DXElementAttribute>(objectType);
            return attr?.Type ?? string.Empty;
        }

        public static string GetTypeName(this DXUnit dxUnit)
            => dxUnit is null ? string.Empty : GetDXUnitTypeName(dxUnit.GetType());

        // ----- Element type discovery -----
        private static Type FindElementType(Type type)
        {
            if (type is null) return typeof(object);
            return ElementTypeCache.GetOrAdd(type, static t =>
            {
                if (t.IsArray) return t.GetElementType()!;
                if (ImplIEnumT(t)) return t.GetGenericArguments()[0];

                var viaIface = t.GetInterfaces().FirstOrDefault(ImplIEnumT);
                if (viaIface is not null) return viaIface.GetGenericArguments()[0];

                if (IsIEnum(t) || t.GetInterfaces().Any(IsIEnum)) return typeof(object);
                return t;
            });
        }

        private static bool ImplIEnumT(Type t) =>
            t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>);

        private static bool IsIEnum(Type t) => t == typeof(System.Collections.IEnumerable);

        public static T? GetAttribute<T>(MemberInfo member, bool inherit = true) where T : Attribute
        {
            if (member is null) return default;

            return member switch
            {
                Type t => GetAttribute<T>(t, inherit),
                PropertyInfo pi => GetAttribute<T>(pi, inherit),
                _ => member.GetCustomAttribute(typeof(T), inherit) as T
            };
        }
    }
}
