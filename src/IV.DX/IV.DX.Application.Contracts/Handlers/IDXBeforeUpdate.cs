using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Handlers
{
    public interface IDXBeforeUpdate<T> : IDXBeforeOrdered where T : DXUnit
    {
        Task<DXResult<T>> BeforeUpdateAsync(T dxUnit, IDXHandlerContext ctx, CancellationToken ct);
    }
}
