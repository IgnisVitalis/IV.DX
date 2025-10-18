using IV.DX.Kernel.Models;

namespace IV.DX.Persistence.Contracts.Abstractions
{
    internal interface IDXEnumCoreRepository
    {
        IEnumerable<DXModel> GetItems(string enumType);
    }
}
