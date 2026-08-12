using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Services
{
    /// <summary>
    /// Write half of the DTO services. Both <see cref="DXUnitDtoService{TRequest, TResponse, TUnit, TMapper}"/>
    /// and <see cref="DXUnitCommandService{TRequest, TUnit, TWriteMapper}"/> delegate here, since their
    /// mappers share no common base and would otherwise carry identical implementations.
    /// </summary>
    internal sealed class DXUnitWriteOperations<TRequest, TUnit>(
        IDXUnitDataService dataService,
        Func<TRequest, CancellationToken, Task<TUnit>> toUnitAsync)
        where TUnit : DXUnit, new()
    {
        public async Task<Guid> CreateAsync(TRequest dto, CancellationToken ct = default)
        {
            var unit = await toUnitAsync(dto, ct);
            return await dataService.InsertAsync(unit, ct: ct);
        }

        public async Task<bool> UpdateAsync(TRequest dto, CancellationToken ct = default)
        {
            var unit = await toUnitAsync(dto, ct);

            // Access is enforced inside UpdateAsync, which reports a missing record as Guid.Empty.
            // Checking existence here instead would demand Read access the caller may not hold.
            return await dataService.UpdateAsync(unit, ct: ct) != Guid.Empty;
        }

        public async Task<Guid> SaveAsync(TRequest dto, CancellationToken ct = default)
        {
            var unit = await toUnitAsync(dto, ct);
            return await dataService.InsertOrUpdateAsync(unit, ct: ct);
        }

        public Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var unit = new TUnit { Id = id };
            return dataService.DeleteAsync(unit, ct: ct);
        }
    }
}
