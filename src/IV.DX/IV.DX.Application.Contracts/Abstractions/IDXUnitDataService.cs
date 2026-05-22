using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Application.Contracts.Abstractions
{
    public interface IDXUnitDataService
    {
        Task<Guid> InsertAsync<T>(T dxUnit, DXHandlerBaseContext? context = default, CancellationToken ct = default) where T : DXUnit, new();
        Task<Guid> UpdateAsync<T>(T dxUnit, DXHandlerBaseContext? context = default, CancellationToken ct = default) where T : DXUnit, new();
        Task<Guid> InsertOrUpdateAsync<T>(T dxUnit, DXHandlerBaseContext? context = default, CancellationToken ct = default) where T : DXUnit, new();
        Task<bool> DeleteAsync<T>(T dxUnit, DXHandlerBaseContext? context = default, CancellationToken ct = default) where T : DXUnit, new();
        Task<Guid> InsertAsync(JObject jObject, DXHandlerBaseContext? context = default, CancellationToken ct = default);
        Task<Guid> UpdateAsync(JObject jObject, DXHandlerBaseContext? context = null, CancellationToken ct = default);
        Task<bool> DeleteAsync(JObject jObject, DXHandlerBaseContext? context = default, CancellationToken ct = default);
        Task<Guid> InsertOrUpdateAsync(JObject jObject, DXHandlerBaseContext? context = null, CancellationToken ct = default);

        Task<Guid> InsertAsync(DXDataBlock<DXUnitRecord> block, DXHandlerBaseContext? context = default, CancellationToken ct = default);
        Task<Guid> UpdateAsync(DXDataBlock<DXUnitRecord> block, DXHandlerBaseContext? context = null, CancellationToken ct = default);
        Task<bool> DeleteAsync(DXDataBlock<DXUnitRecord> block, DXHandlerBaseContext? context = default, CancellationToken ct = default);
        Task<IEnumerable<Guid>> InsertOrUpdateAsync(DXDataBlock<DXUnitRecord> block, DXHandlerBaseContext? context = null, CancellationToken ct = default);




        Task<bool> IsItemExistingAsync(string type, Guid id, DXHandlerBaseContext? context = default, CancellationToken ct = default);
    }
}
