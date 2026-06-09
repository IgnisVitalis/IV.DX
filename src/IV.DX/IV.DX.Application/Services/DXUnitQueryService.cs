using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Services
{
    internal sealed class DXUnitQueryService<TResponse, TUnit, TReadMapper>(
        IDXUnitDataReader reader,
        TReadMapper mapper)
        : IDXUnitQueryService<TResponse>
        where TUnit : DXUnit, new()
        where TReadMapper : DXUnitReadMapper<TResponse, TUnit>
    {
        public async Task<TResponse?> GetAsync(Guid id, CancellationToken ct = default)
        {
            var unit = await reader.GetItemAsync<TUnit>(id, ct: ct);
            return unit is null ? default : await mapper.ToDtoAsync(unit, ct);
        }

        public async Task<IEnumerable<TResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var units = await reader.GetItemsAsync<TUnit>(ct: ct);
            return await MapManyAsync(units, ct);
        }

        public async Task<IEnumerable<TResponse>> GetAsync(string filter, CancellationToken ct = default)
        {
            var units = await reader.GetItemsAsync<TUnit>(filter, ct: ct);
            return await MapManyAsync(units, ct);
        }

        private async Task<IEnumerable<TResponse>> MapManyAsync(IEnumerable<TUnit> units, CancellationToken ct)
        {
            var result = new List<TResponse>();
            foreach (var unit in units)
                result.Add(await mapper.ToDtoAsync(unit, ct));
            return result;
        }
    }
}
