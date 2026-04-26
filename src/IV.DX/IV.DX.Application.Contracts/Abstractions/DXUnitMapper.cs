using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Abstractions
{
    public abstract class DXUnitMapper<TDto, TUnit> where TUnit : DXUnit, new()
    {
        public abstract Task<TDto> ToDtoAsync(TUnit unit, CancellationToken ct = default);
        public abstract Task<TUnit> ToUnitAsync(TDto dto, CancellationToken ct = default);
    }
}
