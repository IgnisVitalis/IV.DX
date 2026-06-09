using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Abstractions
{
    public abstract class DXUnitReadMapper<TResponse, TUnit> where TUnit : DXUnit, new()
    {
        public abstract Task<TResponse> ToDtoAsync(TUnit unit, CancellationToken ct = default);
    }
}
