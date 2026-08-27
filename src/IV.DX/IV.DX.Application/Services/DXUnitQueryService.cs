using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Services
{
    internal sealed class DXUnitQueryService<TResponse, TUnit, TReadMapper>(
        IDXUnitDataReader reader,
        IDXOwnershipReader ownershipReader,
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

        public async Task<IEnumerable<TResponse>> GetOwnedAsync(
            DXUnitTypeAccessOperation operation = DXUnitTypeAccessOperation.Read,
            CancellationToken ct = default)
        {
            var ownedIds = await ownershipReader.GetOwnedIdsAsync<TUnit>(operation, ct);
            if (ownedIds.Count == 0)
                return [];

            // Read back through the unit reader rather than reaching for the pipeline directly, so
            // the get handlers registered for TUnit fire here exactly as they do for every other
            // method on this service. It re-applies the access gate over ids ownership has already
            // vouched for, which costs one indexed lookup, and only when the gate narrows at all.
            var units = await reader.GetItemsAsync<TUnit>(ownedIds, ct: ct);
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
