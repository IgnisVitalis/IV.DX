using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Application.Services
{
    internal class DXElementDataService(
        IDXElementGenericRepository dxElementGenericRepo,
        IDXElementCoreRepository dxElementCoreRepository) : IDXElementDataService
    {
        public Task<IEnumerable<T>> GetItemsAsync<T>(string dxUnitTypeName, string dxFilter, CancellationToken ct = default) where T : DXElement, new()
        {
            return Task.FromResult(dxElementGenericRepo.GetItems<T>(dxUnitTypeName, dxFilter));
        }

        public Task<DXDataBlock<DXElementRecord>> InsertOrUpdateAsync(DXDataBlock<DXElementRecord> block, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(block);

            dxElementCoreRepository.InsertOrUpdate(block);

            return Task.FromResult(block);
        }
    }
}
