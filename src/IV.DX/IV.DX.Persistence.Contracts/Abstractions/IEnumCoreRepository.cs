using IV.DX.Kernel.Models;

namespace IV.DX.Persistence.Contracts.Abstractions
{
    internal interface IEnumCoreRepository
    {
        ESQLMultiItem Get(ESQLBlockDefinition container);
    }
}
