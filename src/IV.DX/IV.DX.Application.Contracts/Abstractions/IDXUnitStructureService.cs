using IV.DX.Application.Contracts.Models;

namespace IV.DX.Application.Contracts.Abstractions
{
    public interface IDXUnitStructureService
    {
        Task<DXUnitDefinitionStructure> GetAsync(string name, CancellationToken ct = default);
    }
}