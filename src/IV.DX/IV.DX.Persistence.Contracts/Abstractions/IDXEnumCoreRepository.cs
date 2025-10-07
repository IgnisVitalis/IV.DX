using IV.DX.Kernel.Models;

namespace IV.DX.Persistence.Contracts.Abstractions
{
    internal interface IDXEnumCoreRepository
    {
        ESQLMultiItem Get(ESQLBlockDefinition container);
    }
}
