using IV.DX.Application.Contracts.Actions;
using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;
using System.Reflection;

namespace IV.DX.Application.Actions
{
    internal static class DXActionScanner
    {
        public static IEnumerable<Type> FindActionTypes(IEnumerable<Assembly> assemblies)
        {
            return assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => !t.IsAbstract
                         && t.IsSubclassOf(typeof(DXActionBase))
                         && t.GetCustomAttribute<DXActionAttribute>() is not null);
        }

        public static DXActionKind ResolveKind(Type actionType)
        {
            if (actionType.IsSubclassOf(typeof(DXUnitActionBase)))
                return DXActionKind.DXUnit;

            return DXActionKind.Generic;
        }
    }
}
