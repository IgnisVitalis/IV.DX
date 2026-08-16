using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Services
{
    internal sealed class DXElementDtoService<TRequest, TResponse, TElement, TUnit, TMapper>
        : IDXElementDtoService<TRequest, TResponse>
        where TElement : DXElement, new()
        where TUnit : DXUnit, new()
        where TMapper : DXElementMapper<TRequest, TResponse, TElement, TUnit>
    {
        private static readonly string UnitTypeName = AttributeReader.GetDXUnitTypeName(typeof(TUnit));

        private readonly IDXElementDataService _dataService;
        private readonly TMapper _mapper;
        private readonly DXElementWriteOperations<TRequest, TElement, TUnit> _write;

        public DXElementDtoService(IDXElementDataService dataService, TMapper mapper)
        {
            _dataService = dataService;
            _mapper = mapper;
            _write = new DXElementWriteOperations<TRequest, TElement, TUnit>(dataService, mapper.ToElementAsync);
        }

        public async Task<TResponse?> GetAsync(Guid id, CancellationToken ct = default)
        {
            var element = await _dataService.GetItemAsync<TElement>(UnitTypeName, id, ct);
            return element is null ? default : await _mapper.ToDtoAsync(element, ct);
        }

        public async Task<TResponse?> GetAsync(Guid dxUnitId, Guid id, CancellationToken ct = default)
        {
            var element = await _dataService.GetItemAsync<TElement>(UnitTypeName, dxUnitId, id, ct);
            return element is null ? default : await _mapper.ToDtoAsync(element, ct);
        }

        public async Task<IEnumerable<TResponse>> GetByUnitAsync(Guid dxUnitId, CancellationToken ct = default)
        {
            var elements = await _dataService.GetItemsByUnitAsync<TElement>(UnitTypeName, dxUnitId, ct);

            var result = new List<TResponse>();
            foreach (var element in elements)
                result.Add(await _mapper.ToDtoAsync(element, ct));

            return result;
        }

        public Task<Guid> CreateAsync(Guid dxUnitId, TRequest dto, CancellationToken ct = default)
            => _write.CreateAsync(dxUnitId, dto, ct);

        public Task<bool> UpdateAsync(Guid id, TRequest dto, CancellationToken ct = default)
            => _write.UpdateAsync(id, dto, ct);

        public Task<bool> UpdateAsync(Guid dxUnitId, Guid id, TRequest dto, CancellationToken ct = default)
            => _write.UpdateAsync(dxUnitId, id, dto, ct);

        public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
            => _write.DeleteAsync(id, ct);

        public Task<bool> DeleteAsync(Guid dxUnitId, Guid id, CancellationToken ct = default)
            => _write.DeleteAsync(dxUnitId, id, ct);
    }
}
