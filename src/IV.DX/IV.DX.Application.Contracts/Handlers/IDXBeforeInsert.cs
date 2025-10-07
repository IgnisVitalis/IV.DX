using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Handlers
{
    public interface IDXBeforeInsert<T> : IDXBeforeOrdered where T : DXUnit
    {
        Task<DXResult<T>> BeforeInsertAsync(T dxUnit, IDXHandlerContext ctx, CancellationToken ct);
    }
}
