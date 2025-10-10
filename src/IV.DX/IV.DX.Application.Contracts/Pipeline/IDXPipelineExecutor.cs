using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Pipeline
{
    public interface IDXPipelineExecutor
    {
        Task<DXResult<T?>> GetAsync<T>(
            Guid id,
            IDXHandlerContext ctx,
            CancellationToken ct) where T : DXUnit, new();

        Task<DXResult<DXModel?>> GetAsync(
            string typeName,
            Guid id,
            IDXHandlerContext ctx,
            CancellationToken ct);

        Task<DXResult<T>> InsertAsync<T>(
            T dxUnit,
            IDXHandlerContext ctx,
            CancellationToken ct) where T : DXUnit, new();

        Task<DXResult<DXModel>> InsertAsync(
            DXModel dxModel,
            IDXHandlerContext ctx,
            CancellationToken ct);

        Task<DXResult<T>> UpdateAsync<T>(
            T dxUnit,
            IDXHandlerContext ctx,
            CancellationToken ct) where T : DXUnit, new();

        Task<DXResult<DXModel>> UpdateAsync(
           DXModel dxModel,
           IDXHandlerContext ctx,
           CancellationToken ct);

        Task<DXResult<T>> DeleteAsync<T>(
            T dxUnit,
            IDXHandlerContext ctx,
            CancellationToken ct) where T : DXUnit, new();

        Task<DXResult<DXModel>> DeleteAsync(
            DXModel dxModel,
            IDXHandlerContext ctx,
            CancellationToken ct);
    }
}
