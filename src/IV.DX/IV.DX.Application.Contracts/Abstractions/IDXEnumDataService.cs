using IV.DX.Kernel.Models;

using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Abstractions
{
    public interface IDXEnumDataService
    {
        Task<IDictionary<int, string>> GetItemsAsync(string enumTypeName, CancellationToken ct = default);
        Task<DXDataBlock<DXEnumRecord>> InsertOrUpdateAsync(DXDataBlock<DXEnumRecord> block, CancellationToken ct = default);
    }
}
