using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Services
{
    internal sealed class DXElementQueryService<TResponse, TElement, TUnit, TReadMapper>(
        IDXElementDataService dataService,
        TReadMapper mapper)
        : IDXElementQueryService<TResponse>
        where TElement : DXElement, new()
        where TUnit : DXUnit, new()
        where TReadMapper : DXElementReadMapper<TResponse, TElement, TUnit>
    {
        private static readonly string UnitTypeName = AttributeReader.GetDXUnitTypeName(typeof(TUnit));

        public async Task<TResponse?> GetAsync(Guid id, CancellationToken ct = default)
        {
            var element = await dataService.GetItemAsync<TElement>(UnitTypeName, id, ct);
            return element is null ? default : await mapper.ToDtoAsync(element, ct);
        }

        public async Task<TResponse?> GetAsync(Guid dxUnitId, Guid id, CancellationToken ct = default)
        {
            var element = await dataService.GetItemAsync<TElement>(UnitTypeName, dxUnitId, id, ct);
            return element is null ? default : await mapper.ToDtoAsync(element, ct);
        }

        public async Task<IEnumerable<TResponse>> GetByUnitAsync(Guid dxUnitId, CancellationToken ct = default)
        {
            var elements = await dataService.GetItemsByUnitAsync<TElement>(UnitTypeName, dxUnitId, ct);

            var result = new List<TResponse>();
            foreach (var element in elements)
                result.Add(await mapper.ToDtoAsync(element, ct));

            return result;
        }
    }
}
