using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Abstractions
{
    public interface IDXElementDataService
    {
        Task<T> GetItems<T>(string dxFilter, CancellationToken ct = default) where T : DXElement, new();
    }
}