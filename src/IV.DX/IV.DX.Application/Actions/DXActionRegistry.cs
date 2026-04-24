using IV.DX.Application.Contracts.Actions;
using IV.DX.Kernel.Attributes;
using System.Reflection;

namespace IV.DX.Application.Actions
{
    internal sealed class DXActionRegistry : IDXActionRegistry
    {
        private readonly Dictionary<(string Module, string Key), Type> _actions = new();
        private readonly object _lock = new();

        public void Register(Type actionType)
        {
            var attr = actionType.GetCustomAttribute<DXActionAttribute>()
                ?? throw new InvalidOperationException(
                    $"Type {actionType.FullName} must have [{nameof(DXActionAttribute)}].");

            if (!actionType.IsSubclassOf(typeof(DXActionBase)))
                throw new InvalidOperationException(
                    $"Type {actionType.FullName} must inherit from {nameof(DXActionBase)}.");

            lock (_lock)
            {
                _actions[(attr.Module, attr.Key)] = actionType;
            }
        }

        public Type? Resolve(string module, string key)
        {
            lock (_lock)
            {
                _actions.TryGetValue((module, key), out var type);
                return type;
            }
        }
    }
}
