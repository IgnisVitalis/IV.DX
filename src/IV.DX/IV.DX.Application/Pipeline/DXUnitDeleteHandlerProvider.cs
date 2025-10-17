using IV.DX.Application.Contracts.Handlers;
using IV.DX.Application.Contracts.Pipeline;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Models;
using System.Reflection;

namespace IV.DX.Application.Pipeline
{
    internal class DXUnitDeleteHandlerProvider : IDXUnitDeleteHandlerProvider
    {
        private static readonly Dictionary<string, Type> _typesByName =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<Type, List<object>> _beforeDelete = new();
        private readonly Dictionary<Type, List<object>> _afterDelete = new();

        private readonly object _lock = new();

        public static void InitCore(params Assembly[] scanAssemblies)
        {
            _typesByName.Clear();

            var units = scanAssemblies
                .SelectMany(a => a.DefinedTypes)
                .Where(t => !t.IsAbstract && typeof(DXUnit).IsAssignableFrom(t))
                .Cast<Type>();

            foreach (var t in units)
            {
                var alias = DXTypeName.Get(t);
                _typesByName[alias] = t;
                _typesByName[t.Name] = t;
                if (t.FullName is not null) _typesByName[t.FullName] = t;
            }
        }

        private static void EnsureAliases(Type key)
        {
            var alias = DXTypeName.Get(key);
            _typesByName.TryAdd(alias, key);
            _typesByName.TryAdd(key.Name, key);
            if (key.FullName is not null) _typesByName.TryAdd(key.FullName, key);
        }

        public bool TryResolveType(string typeName, out Type type)
            => _typesByName.TryGetValue(typeName, out type!);

        public void Register<T>(IDXBeforeDeleteHandler<T> handler) where T : DXUnit
        {
            var key = typeof(T);
            lock (_lock)
            {
                if (!_beforeDelete.TryGetValue(key, out var list))
                    _beforeDelete[key] = list = new List<object>();

                var incomingIsUnique = handler is IDXUniqueBeforeDeleteHandler;
                var existsUnique = list.Any(h => h is IDXUniqueBeforeDeleteHandler);

                if (incomingIsUnique && list.Count > 0)
                    throw new InvalidOperationException(
                        $"BeforeDelete handler for {key.Name} must be unique, " +
                        $"but already registered: {string.Join(", ", list.Select(x => x.GetType().Name))}");

                if (!incomingIsUnique && existsUnique)
                    throw new InvalidOperationException(
                        $"BeforeDelete for {key.Name} already has a unique handler; " +
                        $"cannot add '{handler.GetType().Name}'.");

                list.Add(handler);
            }
            EnsureAliases(key);
        }

        public void Register<T>(IDXAfterDeleteHandler<T> handler) where T : DXUnit
        {
            var key = typeof(T);
            lock (_lock)
            {
                if (!_afterDelete.TryGetValue(key, out var list))
                    _afterDelete[key] = list = new List<object>();

                var incomingIsUnique = handler is IDXUniqueAfterDeleteHandler;
                var existsUnique = list.Any(h => h is IDXUniqueAfterDeleteHandler);

                if (incomingIsUnique && list.Count > 0)
                    throw new InvalidOperationException(
                        $"AfterDelete handler for {key.Name} must be unique, " +
                        $"but already registered: {string.Join(", ", list.Select(x => x.GetType().Name))}");

                if (!incomingIsUnique && existsUnique)
                    throw new InvalidOperationException(
                        $"AfterDelete for {key.Name} already has a unique handler; " +
                        $"cannot add '{handler.GetType().Name}'.");

                list.Add(handler);
            }
            EnsureAliases(key);
        }

        public IEnumerable<IDXBeforeDeleteHandler<T>> GetBeforeDeleteHandlers<T>() where T : DXUnit
        {
            var key = typeof(T);
            lock (_lock)
            {
                if (!_beforeDelete.TryGetValue(key, out var list))
                    return Enumerable.Empty<IDXBeforeDeleteHandler<T>>();

                return list
                    .OfType<IDXBeforeDeleteHandler<T>>()
                    .OrderBy(h => (h as IDXBeforeOrdered)?.BeforeOrder ?? 0)
                    .ThenBy(h => h.GetType().FullName)
                    .ToArray();
            }
        }

        public IEnumerable<IDXAfterDeleteHandler<T>> GetAfterDeleteHandlers<T>() where T : DXUnit
        {
            var key = typeof(T);
            lock (_lock)
            {
                if (!_afterDelete.TryGetValue(key, out var list))
                    return Enumerable.Empty<IDXAfterDeleteHandler<T>>();

                return list
                    .OfType<IDXAfterDeleteHandler<T>>()
                    .OrderBy(h => (h as IDXAfterOrdered)?.AfterOrder ?? 0)
                    .ThenBy(h => h.GetType().FullName)
                    .ToArray();
            }
        }
    }

}