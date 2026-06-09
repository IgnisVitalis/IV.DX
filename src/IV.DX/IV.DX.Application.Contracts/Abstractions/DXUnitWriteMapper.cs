using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Abstractions
{
    public abstract class DXUnitWriteMapper<TRequest, TUnit> where TUnit : DXUnit, new()
    {
        public abstract Task<TUnit> ToUnitAsync(TRequest dto, CancellationToken ct = default);
    }
}
