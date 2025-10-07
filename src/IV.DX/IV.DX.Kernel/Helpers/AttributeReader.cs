using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;
using System.Reflection;

namespace IV.DX.Kernel.Helpers
{
    internal static class AttributeReader
    {
        public static T GetSingleAttribute<T>(PropertyInfo propertyInfo) where T : Attribute
        {
            if (propertyInfo == null)
                return null;

            var attribute =
                propertyInfo
                .GetCustomAttribute(typeof(T)) as T;

            return attribute;
        }

        public static T GetSingleAttribute<T>(Type type)
            where T : Attribute
        {
            if (type == null)
                return null;

            var attribute =
               type.GetCustomAttribute(typeof(T)) as T;

            return attribute;
        }

        public static T GetSinglePropertyAttribute<T>(PropertyInfo propertyInfo)
            where T : Attribute
        {
            if (propertyInfo == null)
                return null;

            var attribute =
                propertyInfo
                .GetCustomAttribute(typeof(T)) as T;

            return attribute;
        }

        public static IEnumerable<T> GetAllSinglePropertyAttributes<T>(Type type)
            where T : Attribute
        {
            if (type == null)
                return null;

            var attributes = type.GetProperties()
                        .Select(x => GetSinglePropertyAttribute<T>(x))
                        .Where(x => x != null);

            return attributes;
        }

        public static IEnumerable<PropertyInfo> GetSingleItemInfos(DXUnit esqlObject)
        {
            if (esqlObject == null)
                return null;

            return GetSingleItemInfos(esqlObject.GetType()).Where(x => x.GetValue(esqlObject) != null);
        }

        public static IEnumerable<PropertyInfo> GetSingleItemInfos(Type type)
        {
            var singleFragmentProperties =
              GetProperties(type)
              .Where(x => x.PropertyType.BaseType == typeof(DXElement)).ToList();

            return singleFragmentProperties;
        }

        public static IEnumerable<PropertyInfo> GetMultiItemInfos(DXUnit esqlObject)
        {
            if (esqlObject == null)
                return null;

            return GetMultiItemInfos(esqlObject.GetType()).Where(x => x.GetValue(esqlObject) != null);
        }

        public static IEnumerable<PropertyInfo> GetMultiItemInfos(Type type)
        {
            var multiFragmentProperties =
                GetProperties(type)
                .Where(x => x.PropertyType.IsGenericType).ToList()
                .Where(x => x.PropertyType.GetGenericTypeDefinition() == typeof(DXMultiElementsContainer<>)).ToList();

            return multiFragmentProperties;
        }

        private static PropertyInfo[] GetProperties(Type type)
        {
            return type?.GetProperties();
        }

        private static PropertyInfo[] GetDeclaredOnlyProperties(Type type)
        {
            return type?.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
        }

        public static string GetESQLObjectTypeName(Type type)
        {
            var objectType = FindElementType(type);

            var dataDefinitionNameForFragment = GetAttribute<DXUnitAttribute>(objectType);

            if (dataDefinitionNameForFragment == null)
                return string.Empty;

            return dataDefinitionNameForFragment.ObjectName;
        }

        public static string GetESQLBlockTypeName(Type type)
        {
            var objectType = FindElementType(type);

            var attribute = GetAttribute<DXElementAttribute>(objectType);

            if (attribute == null)
                return string.Empty;

            return attribute.BlockName;
        }

        public static string GetTypeName(this DXUnit esqlObject)
        {
            return GetESQLObjectTypeName(esqlObject.GetType());
        }


        public static T GetAttribute<T>(Type configurationType) where T : Attribute
        {
            var attribute =
                configurationType
                .GetCustomAttribute(typeof(T)) as T;

            return attribute;
        }

        /// <summary>Finds the type of the element of a type. Returns null if this type does not enumerate.</summary>
        /// <param name="type">The type to check.</param>
        /// <returns>The element type, if found; otherwise, <see langword="null"/>.</returns>
        private static Type FindElementType(Type type)
        {
            if (type.IsArray)
                return type.GetElementType();

            // type is IEnumerable<T>;
            if (ImplIEnumT(type))
                return type.GetGenericArguments().First();

            // type implements/extends IEnumerable<T>;
            var enumType = type.GetInterfaces().Where(ImplIEnumT).Select(t => t.GetGenericArguments().First()).FirstOrDefault();
            if (enumType != null)
                return enumType;

            // type is IEnumerable
            if (IsIEnum(type) || type.GetInterfaces().Any(IsIEnum))
                return typeof(object);

            // Return target type
            return type;
        }

        private static bool ImplIEnumT(Type t)
        {
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>);
        }

        private static bool IsIEnum(Type t)
        {
            return t == typeof(System.Collections.IEnumerable);
        }

        public static T GetAttribute<T>(PropertyInfo propertyInfo) where T : Attribute
        {
            var attribute =
                propertyInfo
                .GetCustomAttribute(typeof(T)) as T;

            return attribute;
        }
    }
}