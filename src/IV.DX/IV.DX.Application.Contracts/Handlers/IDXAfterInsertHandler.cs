using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Handlers
{
    public interface IDXAfterInsertHandler<T> : IDXAfterOrdered where T : DXUnit
    {
        Task<DXResult> AfterInsertAsync(T dxUnit, IDXHandlerContext ctx, CancellationToken ct);
    }
}