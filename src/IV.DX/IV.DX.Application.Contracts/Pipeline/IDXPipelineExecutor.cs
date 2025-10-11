using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Application.Contracts.Pipeline
{
    public interface IDXPipelineExecutor
    {
        Task<DXResult<bool>> IsUnitExistingAsync<T>(
            Guid id,
            IDXHandlerContext ctx,
            CancellationToken ct) where T : DXUnit, new();

        Task<DXResult<bool>> IsUnitExistingAsync(
            string typeName,
            Guid id,
            IDXHandlerContext ctx,
            CancellationToken ct);

        Task<DXResult<T?>> GetAsync<T>(
            Guid id,
            IDXHandlerContext ctx,
            CancellationToken ct) where T : DXUnit, new();

        Task<DXResult<IEnumerable<T>?>> GetItemsAsync<T>(
           IEnumerable<Guid> ids,
           IDXHandlerContext ctx,
           CancellationToken ct) where T : DXUnit, new();

        Task<DXResult<IEnumerable<T>?>> GetItemsAsync<T>(
            string query,
            IDXHandlerContext ctx,
            CancellationToken ct) where T : DXUnit, new();

        Task<DXResult<IEnumerable<T>?>> GetItemsAsync<T>(
           IDXHandlerContext ctx,
           CancellationToken ct) where T : DXUnit, new();

        Task<DXResult<JObject?>> GetAsync(
            string typeName,
            Guid id,
            IDXHandlerContext ctx,
            CancellationToken ct);

        Task<DXResult<IEnumerable<JObject>?>> GetItemsAsync(
            string typeName,
            IEnumerable<Guid> ids,
            IDXHandlerContext ctx,
            CancellationToken ct);

        Task<DXResult<IEnumerable<JObject>?>> GetItemsAsync(
            string typeName,
            string query,
            IDXHandlerContext ctx,
            CancellationToken ct);

        Task<DXResult<IEnumerable<JObject>?>> GetItemsAsync(
            string typeName,
            IDXHandlerContext ctx,
            CancellationToken ct);

        Task<DXResult<T>> InsertAsync<T>(
            T dxUnit,
            IDXHandlerContext ctx,
            CancellationToken ct) where T : DXUnit, new();

        Task<DXResult<JObject>> InsertAsync(
            JObject dxModel,
            IDXHandlerContext ctx,
            CancellationToken ct);

        Task<DXResult<T>> UpdateAsync<T>(
            T dxUnit,
            IDXHandlerContext ctx,
            CancellationToken ct) where T : DXUnit, new();

        Task<DXResult<JObject>> UpdateAsync(
           JObject dxModel,
           IDXHandlerContext ctx,
           CancellationToken ct);

        Task<DXResult<T>> DeleteAsync<T>(
            T dxUnit,
            IDXHandlerContext ctx,
            CancellationToken ct) where T : DXUnit, new();

        Task<DXResult<JObject>> DeleteAsync(
            JObject dxModel,
            IDXHandlerContext ctx,
            CancellationToken ct);
    }
}
