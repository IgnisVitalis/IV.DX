using IV.DX.Application.Contracts.Handlers;
using IV.DX.Application.Contracts.Pipeline;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Models;
using System.Reflection;

namespace IV.DX.Application.Pipeline
{
    internal class DXUnitGetHandlerProvider : IDXUnitGetHandlerProvider
    {
        private static readonly Dictionary<string, Type> _typesByName =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<Type, List<object>> _beforeGet = new();
        private readonly Dictionary<Type, List<object>> _afterGet = new();
        private readonly Dictionary<Type, List<object>> _isItemExisting = new();

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

        // ---------------- Register: BEFORE GET ----------------

        public void Register<T>(IDXBeforeGetHandler<T> handler) where T : DXUnit
        {
            var key = typeof(T);
            lock (_lock)
            {
                if (!_beforeGet.TryGetValue(key, out var list))
                    _beforeGet[key] = list = new List<object>();
             
                var isUniqueIncoming = handler is IDXUniqueBeforeGetHandler;
                var existsUnique = list.Any(h => h is IDXUniqueBeforeGetHandler);

                if (isUniqueIncoming && list.Count > 0)
                    throw new InvalidOperationException(
                        $"BeforeGet handler for {key.Name} must be unique, but handlers already registered: " +
                        $"{string.Join(", ", list.Select(x => x.GetType().Name))}");

                if (!isUniqueIncoming && existsUnique)
                    throw new InvalidOperationException(
                        $"BeforeGet handler for {key.Name} is already occupied by a unique handler; cannot add '{handler.GetType().Name}'.");

                list.Add(handler);
            }

            EnsureAliases(key);
        }

        public void Register<T>(IDXAfterGetHadnler<T> handler) where T : DXUnit
        {
            var key = typeof(T);

            lock (_lock)
            {
                if (!_afterGet.TryGetValue(key, out var list))
                    _afterGet[key] = list = new List<object>();

                var isUniqueIncoming = handler is IDXUniqueAfterGetHandler;
                var existsUnique = list.Any(h => h is IDXUniqueAfterGetHandler);

                if (isUniqueIncoming && list.Count > 0)
                    throw new InvalidOperationException(
                        $"AfterGet handler for {key.Name} must be unique, but handlers already registered: " +
                        $"{string.Join(", ", list.Select(x => x.GetType().Name))}");

                if (!isUniqueIncoming && existsUnique)
                    throw new InvalidOperationException(
                        $"AfterGet handler for {key.Name} is already occupied by a unique handler; cannot add '{handler.GetType().Name}'.");

                list.Add(handler);
            }

            EnsureAliases(key);
        }

        public void Register<T>(IDXIsItemExistingHandler<T> handler) where T : DXUnit
        {
            var key = typeof(T);

            lock (_lock)
            {
                if (!_isItemExisting.TryGetValue(key, out var list))
                    _isItemExisting[key] = list = new List<object>();

                var isUniqueIncoming = handler is IDXUniqueIsItemExistingHandler;
                var existsUnique = list.Any(h => h is IDXUniqueIsItemExistingHandler);

                if (isUniqueIncoming && list.Count > 0)
                    throw new InvalidOperationException(
                        $"IsItemExisting handler for {key.Name} must be unique, but handlers already registered: " +
                        $"{string.Join(", ", list.Select(x => x.GetType().Name))}");

                if (!isUniqueIncoming && existsUnique)
                    throw new InvalidOperationException(
                        $"IsItemExisting handler for {key.Name} is already occupied by a unique handler; cannot add '{handler.GetType().Name}'.");

                list.Add(handler);
            }

            EnsureAliases(key);
        }

        public IEnumerable<IDXBeforeGetHandler<T>> GetBeforeGetHandlers<T>() where T : DXUnit
        {
            var key = typeof(T);

            lock (_lock)
            {
                if (!_beforeGet.TryGetValue(key, out var list))
                    return Enumerable.Empty<IDXBeforeGetHandler<T>>();

                return list.OfType<IDXBeforeGetHandler<T>>()
                           .OrderBy(h => (h as IDXBeforeOrdered)?.BeforeOrder ?? 0)
                           .ThenBy(h => h.GetType().FullName)
                           .ToArray();
            }
        }

        public IEnumerable<IDXAfterGetHadnler<T>> GetAfterGetHandlers<T>() where T : DXUnit
        {
            var key = typeof(T);

            lock (_lock)
            {
                if (!_afterGet.TryGetValue(key, out var list))
                    return Enumerable.Empty<IDXAfterGetHadnler<T>>();

                return list.OfType<IDXAfterGetHadnler<T>>()
                           .OrderBy(h => (h as IDXAfterOrdered)?.AfterOrder ?? 0)
                           .ThenBy(h => h.GetType().FullName)
                           .ToArray();
            }
        }

        public IEnumerable<IDXIsItemExistingHandler<T>> GetIsItemExistingHandlers<T>() where T : DXUnit
        {
            var key = typeof(T);

            lock (_lock)
            {
                if (!_isItemExisting.TryGetValue(key, out var list))
                    return Enumerable.Empty<IDXIsItemExistingHandler<T>>();

                return list.OfType<IDXIsItemExistingHandler<T>>()
                           .OrderBy(h => (h as IDXBeforeOrdered)?.BeforeOrder ?? 0)
                           .ThenBy(h => h.GetType().FullName)
                           .ToArray();
            }
        }

        public bool TryResolveType(string typeName, out Type type)
            => _typesByName.TryGetValue(typeName, out type!);
    }
}