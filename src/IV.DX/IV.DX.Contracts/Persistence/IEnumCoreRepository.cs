using IV.DX.Contracts.Common.Models;

namespace IV.DX.Contracts.Common.Helpers
{
    public interface IEnumCoreRepository
    {
        ESQLMultiItem Get(ESQLBlockDefinition container);
    }
}
