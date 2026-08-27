using IV.DX.Kernel.Enums;

namespace IV.DX.Persistence.Contracts.Abstractions
{
    public interface IDXUnitTypeAccessChecker
    {
        void EnsureAccess(string typeName, DXUnitTypeAccessOperation operation);
        DXAccessDecision CheckAccess(string typeName, DXUnitTypeAccessOperation operation);
    }
}

