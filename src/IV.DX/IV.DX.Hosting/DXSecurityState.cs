using IV.DX.Persistence.Contracts.Abstractions;
using System.Threading;

namespace IV.DX.Hosting
{
    internal sealed class DXSecurityState(IDXStructureCache structureCache) : IDXSecurityState
    {
        private const string SecurityMarkerUnitType = "DXRoleUnit";

        private readonly object _gate = new();
        private bool _isEnabled;
        private bool _isLoaded;

        public bool IsEnabled
        {
            get
            {
                if (!Volatile.Read(ref _isLoaded))
                {
                    LoadFromStructure();
                }

                return Volatile.Read(ref _isEnabled);
            }
        }

        public void LoadFromStructure()
        {
            lock (_gate)
            {
                _isEnabled = structureCache.GetDXUnit(SecurityMarkerUnitType) != null;
                _isLoaded = true;
            }
        }

        public void SetEnabled(bool enabled)
        {
            lock (_gate)
            {
                _isEnabled = enabled;
                _isLoaded = true;
            }
        }
    }
}
