using IV.DX.Application.Contracts.Abstractions;

namespace IV.DX.Application
{
    internal sealed class DXModuleRegistry : IDXModuleRegistry
    {
        private readonly HashSet<string> _registered = new(StringComparer.OrdinalIgnoreCase);

        public void Register(string moduleId) => _registered.Add(moduleId);

        public bool IsRegistered(string moduleId) => _registered.Contains(moduleId);
    }
}
