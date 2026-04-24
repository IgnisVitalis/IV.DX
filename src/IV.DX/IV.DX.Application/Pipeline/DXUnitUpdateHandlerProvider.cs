using IV.DX.Application.Contracts.Handlers;
using IV.DX.Application.Contracts.Pipeline;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Pipeline
{
    internal sealed class DXUnitUpdateHandlerProvider : IDXUnitUpdateHandlerProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly DXUnitUpdateHandlerStore _store;

        public DXUnitUpdateHandlerProvider(IServiceProvider serviceProvider, DXUnitUpdateHandlerStore store)
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

        public void Register<T>(IDXBeforeUpdateHandler<T> handler) where T : DXUnit
        {
            var key = typeof(T);
            RegisterHandler(
                key,
                handler.GetType(),
                _store.BeforeUpdateHandlers,
                typeof(IDXUniqueBeforeUpdateHandler),
                "BeforeUpdate");

            EnsureAliases(key);
        }

        public void Register<T>(IDXAfterUpdateHandler<T> handler) where T : DXUnit
        {
            var key = typeof(T);
            RegisterHandler(
                key,
                handler.GetType(),
                _store.AfterUpdateHandlers,
                typeof(IDXUniqueAfterUpdateHandler),
                "AfterUpdate");

            EnsureAliases(key);
        }

        public IEnumerable<IDXBeforeUpdateHandler<T>> GetBeforeUpdateHandlers<T>() where T : DXUnit
        {
            var key = typeof(T);
            Type[] handlerTypes;

            lock (_store.SyncRoot)
            {
                if (!_store.BeforeUpdateHandlers.TryGetValue(key, out var list))
                    return Enumerable.Empty<IDXBeforeUpdateHandler<T>>();

                handlerTypes = list.ToArray();
            }

            return handlerTypes
                .Select(handlerType => (IDXBeforeUpdateHandler<T>)ResolveRequired(handlerType))
                .OrderBy(h => (h as IDXBeforeOrdered)?.BeforeOrder ?? 0)
                .ThenBy(h => h.GetType().FullName)
                .ToArray();
        }

        public IEnumerable<IDXAfterUpdateHandler<T>> GetAfterUpdateHandlers<T>() where T : DXUnit
        {
            var key = typeof(T);
            Type[] handlerTypes;

            lock (_store.SyncRoot)
            {
                if (!_store.AfterUpdateHandlers.TryGetValue(key, out var list))
                    return Enumerable.Empty<IDXAfterUpdateHandler<T>>();

                handlerTypes = list.ToArray();
            }

            return handlerTypes
                .Select(handlerType => (IDXAfterUpdateHandler<T>)ResolveRequired(handlerType))
                .OrderBy(h => (h as IDXAfterOrdered)?.AfterOrder ?? 0)
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
                        $"{operationName} handler for {key.Name} must be unique, " +
                        $"but already registered: {string.Join(", ", list.Select(x => x.Name))}");
                }

                if (!incomingIsUnique && existsUnique)
                {
                    throw new InvalidOperationException(
                        $"{operationName} for {key.Name} already has a unique handler; " +
                        $"cannot add '{handlerType.Name}'.");
                }

                list.Add(handlerType);
            }
        }
    }

    internal sealed class DXUnitUpdateHandlerStore
    {
        public object SyncRoot { get; } = new();
        public Dictionary<string, Type> TypesByName { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<Type, List<Type>> BeforeUpdateHandlers { get; } = new();
        public Dictionary<Type, List<Type>> AfterUpdateHandlers { get; } = new();
    }
}
