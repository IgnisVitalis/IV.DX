using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Handlers
{
    public interface IDXBeforeGetHandler<T> : IDXBeforeOrdered where T : DXUnit
    {
        Task<DXResult<Guid>> BeforeGetAsync(Guid id, DXHandlerBaseContext ctx, CancellationToken ct);
    }
}
