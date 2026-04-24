using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Handlers
{
    public interface IDXAfterUpdateHandler<T> : IDXAfterOrdered where T : DXUnit
    {
        Task<DXResult> AfterUpdateAsync(T dxUnit, DXHandlerBaseContext ctx, CancellationToken ct);
    }
}
