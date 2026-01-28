using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Abstractions
{
    public interface IDXElementDataService
    {
        Task<IEnumerable<T>> GetItemsAsync<T>(string dxUnitTypeName, string dxFilter, CancellationToken ct = default) where T : DXElement, new();
    }
}