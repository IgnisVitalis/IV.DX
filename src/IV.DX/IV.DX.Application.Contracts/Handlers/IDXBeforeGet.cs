using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Handlers
{
    public interface IDXBeforeGet<T> : IDXBeforeOrdered where T : DXUnit
    {
        Task<DXResult<T>> BeforeGetAsync(Guid id, IDXHandlerContext ctx, CancellationToken ct);
    }
}
