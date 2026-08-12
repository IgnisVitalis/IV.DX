using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Services
{
    internal sealed class DXUnitCommandService<TRequest, TUnit, TWriteMapper>
        : IDXUnitCommandService<TRequest>
        where TUnit : DXUnit, new()
        where TWriteMapper : DXUnitWriteMapper<TRequest, TUnit>
    {
        private readonly DXUnitWriteOperations<TRequest, TUnit> _write;

        public DXUnitCommandService(IDXUnitDataService dataService, TWriteMapper mapper)
            => _write = new DXUnitWriteOperations<TRequest, TUnit>(dataService, mapper.ToUnitAsync);

        public Task<Guid> CreateAsync(TRequest dto, CancellationToken ct = default)
            => _write.CreateAsync(dto, ct);

        public Task<bool> UpdateAsync(TRequest dto, CancellationToken ct = default)
            => _write.UpdateAsync(dto, ct);

        public Task<Guid> SaveAsync(TRequest dto, CancellationToken ct = default)
            => _write.SaveAsync(dto, ct);

        public Task DeleteAsync(Guid id, CancellationToken ct = default)
            => _write.DeleteAsync(id, ct);
    }
}
