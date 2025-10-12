using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Models;
namespace IV.DX.Application.Contracts.Handlers
{
    public interface IDXAfterGetHadnler<T> : IDXAfterOrdered where T : DXUnit
    {
        Task<DXResult> AfterGetAsync(T? dxUnit, IDXHandlerContext ctx, CancellationToken ct);
    }
}
