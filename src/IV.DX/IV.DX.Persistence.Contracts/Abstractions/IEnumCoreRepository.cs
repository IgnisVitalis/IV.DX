using IV.DX.Kernel.Models;

namespace IV.DX.Persistence.Contracts.Abstractions
{
    public interface IEnumCoreRepository
    {
        ESQLMultiItem Get(ESQLBlockDefinition container);
    }
}
