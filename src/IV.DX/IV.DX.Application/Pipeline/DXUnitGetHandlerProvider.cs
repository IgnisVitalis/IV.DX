using IV.DX.Application.Contracts.Handlers;
using IV.DX.Application.Contracts.Pipeline;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Pipeline
{
    internal sealed class DXUnitGetHandlerProvider : IDXUnitGetHandlerProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly DXUnitGetHandlerStore _store;

        public DXUnitGetHandlerProvider(IServiceProvider serviceProvider, DXUnitGetHandlerStore store)
        {
            _serviceProvider = serviceProvider;
            _store = store;
        }

        public bool TryResolveType(string typeName, out Type type)
        {
            lock (_store.SyncRoot)
            {
                return _store.TypesByName.TryGetValue(typeName, out type!);
            }
        }

        public void Register<T>(IDXBeforeGetHandler<T> handler) where T : DXUnit
        {
            var key = typeof(T);
            RegisterHandler(
                key,
                handler.GetType(),
                _store.BeforeGetHandlers,
                typeof(IDXUniqueBeforeGetHandler),
                "BeforeGet");

            EnsureAliases(key);
        }

        public void Register<T>(IDXAfterGetHadnler<T> handler) where T : DXUnit
        {
            var key = typeof(T);
            RegisterHandler(
                key,
                handler.GetType(),
                _store.AfterGetHandlers,
                typeof(IDXUniqueAfterGetHandler),
                "AfterGet");

            EnsureAliases(key);
        }

        public void Register<T>(IDXIsItemExistingHandler<T> handler) where T : DXUnit
        {
            var key = typeof(T);
            RegisterHandler(
                key,
                handler.GetType(),
                _store.IsItemExistingHandlers,
                typeof(IDXUniqueIsItemExistingHandler),
                "IsItemExisting");

            EnsureAliases(key);
        }

        public IEnumerable<IDXBeforeGetHandler<T>> GetBeforeGetHandlers<T>() where T : DXUnit
        {
            var key = typeof(T);
            Type[] handlerTypes;

            lock (_store.SyncRoot)
            {
                if (!_store.BeforeGetHandlers.TryGetValue(key, out var list))
                    return Enumerable.Empty<IDXBeforeGetHandler<T>>();

                handlerTypes = list.ToArray();
            }

            return handlerTypes
                .Select(handlerType => (IDXBeforeGetHandler<T>)ResolveRequired(handlerType))
                .OrderBy(h => (h as IDXBeforeOrdered)?.BeforeOrder ?? 0)
                .ThenBy(h => h.GetType().FullName)
                .ToArray();
        }

        public IEnumerable<IDXAfterGetHadnler<T>> GetAfterGetHandlers<T>() where T : DXUnit
        {
            var key = typeof(T);
            Type[] handlerTypes;

            lock (_store.SyncRoot)
            {
                if (!_store.AfterGetHandlers.TryGetValue(key, out var list))
                    return Enumerable.Empty<IDXAfterGetHadnler<T>>();

                handlerTypes = list.ToArray();
            }

            return handlerTypes
                .Select(handlerType => (IDXAfterGetHadnler<T>)ResolveRequired(handlerType))
                .OrderBy(h => (h as IDXAfterOrdered)?.AfterOrder ?? 0)
                .ThenBy(h => h.GetType().FullName)
                .ToArray();
        }

        public IEnumerable<IDXIsItemExistingHandler<T>> GetIsItemExistingHandlers<T>() where T : DXUnit
        {
            var key = typeof(T);
            Type[] handlerTypes;

            lock (_store.SyncRoot)
            {
                if (!_store.IsItemExistingHandlers.TryGetValue(key, out var list))
                    return Enumerable.Empty<IDXIsItemExistingHandler<T>>();

                handlerTypes = list.ToArray();
            }

            return handlerTypes
                .Select(handlerType => (IDXIsItemExistingHandler<T>)ResolveRequired(handlerType))
                .OrderBy(h => (h as IDXBeforeOrdered)?.BeforeOrder ?? 0)
                .ThenBy(h => h.GetType().FullName)
                .ToArray();
        }

        private object ResolveRequired(Type handlerType)
        {
            return _serviceProvider.GetService(handlerType)
                ?? throw new InvalidOperationException($"Handler '{handlerType.FullName}' is not registered.");
        }

        private void EnsureAliases(Type key)
        {
            var alias = DXTypeName.Get(key);

            lock (_store.SyncRoot)
            {
                _store.TypesByName.TryAdd(alias, key);
                _store.TypesByName.TryAdd(key.Name, key);
                if (key.FullName is not null)
                    _store.TypesByName.TryAdd(key.FullName, key);
            }
        }

        private void RegisterHandler(
            Type key,
            Type handlerType,
            Dictionary<Type, List<Type>> registry,
            Type uniqueMarkerInterface,
            string operationName)
        {
            lock (_store.SyncRoot)
            {
                if (!registry.TryGetValue(key, out var list))
                    registry[key] = list = new List<Type>();

                if (list.Contains(handlerType))
                    return;

                var incomingIsUnique = uniqueMarkerInterface.IsAssignableFrom(handlerType);
                var existsUnique = list.Any(uniqueMarkerInterface.IsAssignableFrom);

                if (incomingIsUnique && list.Count > 0)
                {
                    throw new InvalidOperationException(
                        $"{operationName} handler for {key.Name} must be unique, but handlers already registered: " +
                        $"{string.Join(", ", list.Select(x => x.Name))}");
                }

                if (!incomingIsUnique && existsUnique)
                {
                    throw new InvalidOperationException(
                        $"{operationName} handler for {key.Name} is already occupied by a unique handler; cannot add '{handlerType.Name}'.");
                }

                list.Add(handlerType);
            }
        }
    }

    internal sealed class DXUnitGetHandlerStore
    {
        public object SyncRoot { get; } = new();
        public Dictionary<string, Type> TypesByName { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<Type, List<Type>> BeforeGetHandlers { get; } = new();
        public Dictionary<Type, List<Type>> AfterGetHandlers { get; } = new();
        public Dictionary<Type, List<Type>> IsItemExistingHandlers { get; } = new();
    }
}
