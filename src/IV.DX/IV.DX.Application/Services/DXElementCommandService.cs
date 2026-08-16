using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Services
{
    internal sealed class DXElementCommandService<TRequest, TElement, TUnit, TWriteMapper>
        : IDXElementCommandService<TRequest>
        where TElement : DXElement, new()
        where TUnit : DXUnit, new()
        where TWriteMapper : DXElementWriteMapper<TRequest, TElement, TUnit>
    {
        private readonly DXElementWriteOperations<TRequest, TElement, TUnit> _write;

        public DXElementCommandService(IDXElementDataService dataService, TWriteMapper mapper)
            => _write = new DXElementWriteOperations<TRequest, TElement, TUnit>(dataService, mapper.ToElementAsync);

        public Task<Guid> CreateAsync(Guid dxUnitId, TRequest dto, CancellationToken ct = default)
            => _write.CreateAsync(dxUnitId, dto, ct);

        public Task<bool> UpdateAsync(TRequest dto, CancellationToken ct = default)
            => _write.UpdateAsync(dto, ct);

        public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
            => _write.DeleteAsync(id, ct);
    }
}
