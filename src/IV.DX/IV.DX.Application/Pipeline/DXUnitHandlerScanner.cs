using IV.DX.Application.Contracts.Handlers;
using System.Reflection;

namespace IV.DX.Application.Pipeline
{
    internal static class DXUnitHandlerScanner
    {
        private static readonly Type[] _handlerGenericInterfaces =
        {
            typeof(IDXBeforeInsert<>), typeof(IDXAfterInsert<>),
            typeof(IDXBeforeUpdate<>), typeof(IDXAfterUpdate<>),
            typeof(IDXBeforeDelete<>), typeof(IDXAfterDelete<>),
            typeof(IDXBeforeGet<>),    typeof(IDXAfterGet<>),
            typeof(IDXIsItemExisting<>)
        };

        public static IReadOnlyList<Type> FindHandlerTypes(params Assembly[] assemblies)
        {
            return assemblies
                .SelectMany(a => a.DefinedTypes)
                .Where(t =>
                    !t.IsAbstract &&
                    !t.IsInterface &&
                    t.GetInterfaces().Any(IsDXUnitHandlerGeneric))
                .Select(t => (Type)t)
                .Distinct()
                .ToArray();
        }

        private static bool IsDXUnitHandlerGeneric(Type i)
        {
            if (!i.IsGenericType) return false;
            var def = i.GetGenericTypeDefinition();
            return _handlerGenericInterfaces.Contains(def);
        }

        public static IEnumerable<(Type handlerType, Type openInterface, Type unitType)> EnumerateDxHandlerInterfaces(Type handlerType)
        {
            foreach (var i in handlerType.GetInterfaces())
            {
                if (!i.IsGenericType) continue;
                var def = i.GetGenericTypeDefinition();
                if (!_handlerGenericInterfaces.Contains(def)) continue;

                var unitType = i.GetGenericArguments()[0];
                yield return (handlerType, def, unitType);
            }
        }
    }
}
