using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Application.Services
{
    internal class DXElementDataService(IDXElementGenericRepository dxElementGenericRepo) : IDXElementDataService
    {
        public async Task<IEnumerable<T>> GetItemsAsync<T>(string dxUnitTypeName, string dxFilter, CancellationToken ct = default) where T : DXElement, new()
        {
            return dxElementGenericRepo.GetItems<T>(dxUnitTypeName, dxFilter);
        }
    }
}
