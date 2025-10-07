using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Handlers
{
    public interface IDXAfterUpdate<T> : IDXAfterOrdered where T : DXUnit
    {
        Task<DXResult> AfterUpdateAsync(T dxUnit, IDXHandlerContext ctx, CancellationToken ct);
    }
}
