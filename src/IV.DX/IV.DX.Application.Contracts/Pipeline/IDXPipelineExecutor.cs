using IV.DX.Application.Contracts.Handlers;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Pipeline
{
    public interface IDXPipelineExecutor
    {
        Task<DXResult<T?>> GetAsync<T>(
            Guid id,
            IEnumerable<IDXBeforeGet<T>> befores,
            IEnumerable<IDXAfterGet<T>> afters,
            IDXHandlerContext ctx,
            CancellationToken ct) where T : DXUnit;

        Task<DXResult<T>> InsertAsync<T>(
            T model,
            IEnumerable<IDXBeforeInsert<T>> befores,
            IEnumerable<IDXAfterInsert<T>> afters,
            IDXHandlerContext ctx,
            CancellationToken ct) where T : DXUnit;

        Task<DXResult<T>> UpdateAsync<T>(
            T model,
            IEnumerable<IDXBeforeUpdate<T>> befores,
            IEnumerable<IDXAfterUpdate<T>> afters,
            IDXHandlerContext ctx,
            CancellationToken ct) where T : DXUnit;

        Task<DXResult> DeleteAsync<T>(
            Guid id,
            IEnumerable<IDXBeforeDelete<T>> befores,
            IEnumerable<IDXAfterDelete<T>> afters,
            IDXHandlerContext ctx,
            CancellationToken ct) where T : DXUnit;
    }
}
