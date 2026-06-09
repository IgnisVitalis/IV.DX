using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Services
{
    internal sealed class DXUnitDtoService<TDto, TUnit, TMapper>(
        IDXUnitDataReader reader,
        IDXUnitDataService dataService,
        TMapper mapper)
        : IDXUnitDtoService<TDto>
        where TUnit : DXUnit, new()
        where TMapper : DXUnitMapper<TDto, TUnit>
    {
        public async Task<TDto?> GetAsync(Guid id, CancellationToken ct = default)
        {
            var unit = await reader.GetItemAsync<TUnit>(id, ct: ct);
            return unit is null ? default : await mapper.ToDtoAsync(unit, ct);
        }

        public async Task<IEnumerable<TDto>> GetAllAsync(CancellationToken ct = default)
        {
            var units = await reader.GetItemsAsync<TUnit>(ct: ct);
            return await MapManyAsync(units, ct);
        }

        public async Task<IEnumerable<TDto>> GetAsync(string filter, CancellationToken ct = default)
        {
            var units = await reader.GetItemsAsync<TUnit>(filter, ct: ct);
            return await MapManyAsync(units, ct);
        }

        public async Task<Guid> SaveAsync(TDto dto, CancellationToken ct = default)
        {
            var unit = await mapper.ToUnitAsync(dto, ct);
            return await dataService.InsertOrUpdateAsync(unit, ct: ct);
        }

        public Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var unit = new TUnit { Id = id };
            return dataService.DeleteAsync(unit, ct: ct);
        }

        private async Task<IEnumerable<TDto>> MapManyAsync(IEnumerable<TUnit> units, CancellationToken ct)
        {
            var result = new List<TDto>();
            foreach (var unit in units)
                result.Add(await mapper.ToDtoAsync(unit, ct));
            return result;
        }
    }
}
