using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Services
{
    internal sealed class DXUnitCommandService<TRequest, TUnit, TWriteMapper>(
        IDXUnitDataService dataService,
        TWriteMapper mapper)
        : IDXUnitCommandService<TRequest>
        where TUnit : DXUnit, new()
        where TWriteMapper : DXUnitWriteMapper<TRequest, TUnit>
    {
        public async Task<Guid> SaveAsync(TRequest dto, CancellationToken ct = default)
        {
            var unit = await mapper.ToUnitAsync(dto, ct);
            return await dataService.InsertOrUpdateAsync(unit, ct: ct);
        }

        public Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var unit = new TUnit { Id = id };
            return dataService.DeleteAsync(unit, ct: ct);
        }
    }
}
