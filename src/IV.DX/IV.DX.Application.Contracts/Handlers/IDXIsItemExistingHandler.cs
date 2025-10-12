using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Handlers
{
    public interface IDXIsItemExistingHandler<T> : IDXBeforeOrdered where T : DXUnit
    {
        Task<DXResult<bool>> IsItemExistingAsync(Guid id, IDXHandlerContext ctx, CancellationToken ct);
    }
}