using IV.DX.Application.Contracts.Models;

namespace IV.DX.Application.Contracts.Abstractions
{
    public interface IDXUnitStructureService
    {
        Task<DXUnitStructure> GetAsync(string name, CancellationToken ct = default);
    }
}