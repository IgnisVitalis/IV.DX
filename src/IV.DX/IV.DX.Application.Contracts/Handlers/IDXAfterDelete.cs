using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Handlers
{
    public interface IDXAfterDelete<T> : IDXAfterOrdered where T : DXUnit
    {
        Task<DXResult> AfterDeleteAsync(T dxUnit, IDXHandlerContext ctx, CancellationToken ct);
    }
}
