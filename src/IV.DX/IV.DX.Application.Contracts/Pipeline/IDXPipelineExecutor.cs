using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Application.Contracts.Pipeline
{
    public interface IDXPipelineExecutor
    {
        Task<DXResult<bool>> IsUnitExistingAsync<T>(
            Guid id,
            DXHandlerBaseContext ctx,
            CancellationToken ct) where T : DXUnit, new();

        Task<DXResult<bool>> IsUnitExistingAsync(
            string typeName,
            Guid id,
            DXHandlerBaseContext ctx,
            CancellationToken ct);

        Task<DXResult<T?>> GetAsync<T>(
            Guid id,
            DXHandlerBaseContext ctx,
            CancellationToken ct) where T : DXUnit, new();

        Task<DXResult<IEnumerable<T>?>> GetItemsAsync<T>(
           IEnumerable<Guid> ids,
           DXHandlerBaseContext ctx,
           CancellationToken ct) where T : DXUnit, new();

        Task<DXResult<IEnumerable<T>?>> GetItemsAsync<T>(
            string query,
            DXHandlerBaseContext ctx,
            CancellationToken ct) where T : DXUnit, new();

        Task<DXResult<IEnumerable<T>?>> GetItemsAsync<T>(
           DXHandlerBaseContext ctx,
           CancellationToken ct) where T : DXUnit, new();

        Task<DXResult<JObject?>> GetAsync(
            string typeName,
            Guid id,
            DXHandlerBaseContext ctx,
            CancellationToken ct);

        Task<DXResult<JObject?>> GetItemsAsync(
            string typeName,
            IEnumerable<Guid> ids,
            DXHandlerBaseContext ctx,
            CancellationToken ct);

        Task<DXResult<JObject?>> GetItemsAsync(
            string typeName,
            string query,
            DXHandlerBaseContext ctx,
            CancellationToken ct);

        Task<DXResult<JObject?>> GetItemsAsync(
            string typeName,
            DXHandlerBaseContext ctx,
            CancellationToken ct);

        Task<DXResult<Guid>> InsertAsync<T>(
            T dxUnit,
            DXHandlerBaseContext ctx,
            CancellationToken ct) where T : DXUnit, new();

        Task<DXResult<Guid>> InsertAsync(
            JObject dxModel,
            DXHandlerBaseContext ctx,
            CancellationToken ct);

        Task<DXResult<Guid>> UpdateAsync<T>(
            T dxUnit,
            DXHandlerBaseContext ctx,
            CancellationToken ct) where T : DXUnit, new();

        Task<DXResult<Guid>> UpdateAsync(
           JObject dxModel,
           DXHandlerBaseContext ctx,
           CancellationToken ct);

        Task<DXResult<T>> DeleteAsync<T>(
            T dxUnit,
            DXHandlerBaseContext ctx,
            CancellationToken ct) where T : DXUnit, new();

        Task<DXResult<JObject>> DeleteAsync(
            JObject dxModel,
            DXHandlerBaseContext ctx,
            CancellationToken ct);

        Task<DXResult<Guid>> InsertAsync(
            DXDataBlock<DXUnitRecord> block,
            DXHandlerBaseContext ctx,
            CancellationToken ct);

        Task<DXResult<Guid>> UpdateAsync(
            DXDataBlock<DXUnitRecord> block,
            DXHandlerBaseContext ctx,
            CancellationToken ct);

        Task<DXResult<DXDataBlock<DXUnitRecord>>> DeleteAsync(
            DXDataBlock<DXUnitRecord> block,
            DXHandlerBaseContext ctx,
            CancellationToken ct);
    }
}
