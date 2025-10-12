using IV.DX.Application.Contracts.Handlers;
using IV.DX.Application.Contracts.Pipeline;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Models;
using System.Reflection;

namespace IV.DX.Application.Pipeline
{
    internal sealed class DXUnitInsertHandlerProvider : IDXUnitInsertHandlerProvider
    {
        private static readonly Dictionary<string, Type> _typesByName =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<Type, List<object>> _beforeInsert = new();
        private readonly Dictionary<Type, List<object>> _afterInsert = new();

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
        public void Register<T>(IDXBeforeInsertHandler<T> handler) where T : DXUnit
        {
            var key = typeof(T);
            lock (_lock)
            {
                if (!_beforeInsert.TryGetValue(key, out var list))
                    _beforeInsert[key] = list = new List<object>();

                var incomingIsUnique = handler is IDXUniqueBeforeInsertHandler;
                var existsUnique = list.Any(h => h is IDXUniqueBeforeInsertHandler);

                if (incomingIsUnique && list.Count > 0)
                    throw new InvalidOperationException(
                        $"BeforeInsert handler for {key.Name} must be unique, " +
                        $"but already registered: {string.Join(", ", list.Select(x => x.GetType().Name))}");

                if (!incomingIsUnique && existsUnique)
                    throw new InvalidOperationException(
                        $"BeforeInsert for {key.Name} already has a unique handler; " +
                        $"cannot add '{handler.GetType().Name}'.");

                list.Add(handler);
            }
            EnsureAliases(key);
        }

        public void Register<T>(IDXAfterInsertHandler<T> handler) where T : DXUnit
        {
            var key = typeof(T);
            lock (_lock)
            {
                if (!_afterInsert.TryGetValue(key, out var list))
                    _afterInsert[key] = list = new List<object>();

                var incomingIsUnique = handler is IDXUniqueAfterInsertHandler;
                var existsUnique = list.Any(h => h is IDXUniqueAfterInsertHandler);

                if (incomingIsUnique && list.Count > 0)
                    throw new InvalidOperationException(
                        $"AfterInsert handler for {key.Name} must be unique, " +
                        $"but already registered: {string.Join(", ", list.Select(x => x.GetType().Name))}");

                if (!incomingIsUnique && existsUnique)
                    throw new InvalidOperationException(
                        $"AfterInsert for {key.Name} already has a unique handler; " +
                        $"cannot add '{handler.GetType().Name}'.");

                list.Add(handler);
            }
            EnsureAliases(key);
        }

        public IEnumerable<IDXBeforeInsertHandler<T>> GetBeforeInsertHandlers<T>() where T : DXUnit
        {
            var key = typeof(T);

            lock (_lock)
            {
                if (!_beforeInsert.TryGetValue(key, out var list))
                    return Enumerable.Empty<IDXBeforeInsertHandler<T>>();

                return list.OfType<IDXBeforeInsertHandler<T>>()
                           .OrderBy(h => (h as IDXBeforeOrdered)?.BeforeOrder ?? 0)
                           .ThenBy(h => h.GetType().FullName)
                           .ToArray();
            }
        }

        public IEnumerable<IDXAfterInsertHandler<T>> GetAfterInsertHandlers<T>() where T : DXUnit
        {
            var key = typeof(T);

            lock (_lock)
            {
                if (!_afterInsert.TryGetValue(key, out var list))
                    return Enumerable.Empty<IDXAfterInsertHandler<T>>();

                return list.OfType<IDXAfterInsertHandler<T>>()
                           .OrderBy(h => (h as IDXAfterOrdered)?.AfterOrder ?? 0)
                           .ThenBy(h => h.GetType().FullName)
                           .ToArray();
            }
        }
    }

}
