namespace IV.DX.Persistence.Contracts.Abstractions
{
    public interface IDXUnitTypeAccessChecker
    {
        void EnsureAccess(string typeName, DXUnitTypeAccessOperation operation);
    }
}

