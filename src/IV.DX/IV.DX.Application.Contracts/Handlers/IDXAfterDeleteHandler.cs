using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Handlers
{
    public interface IDXAfterDeleteHandler<T> : IDXAfterOrdered where T : DXUnit
    {
        Task<DXResult> AfterDeleteAsync(T dxUnit, DXHandlerBaseContext ctx, CancellationToken ct);
    }
}
