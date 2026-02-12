using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Application.Contracts.Abstractions
{
    public interface IDXUnitDataService
    {
        Task<T> InsertAsync<T>(T dxUnit, DXHandlerBaseContext? context = default, CancellationToken ct = default) where T : DXUnit, new();
        Task<T> UpdateAsync<T>(T dxUnit, DXHandlerBaseContext? context = default, CancellationToken ct = default) where T : DXUnit, new();
        Task<T> InsertOrUpdateAsync<T>(T dxUnit, DXHandlerBaseContext? context = default, CancellationToken ct = default) where T : DXUnit, new();
        Task<bool> DeleteAsync<T>(T dxUnit, DXHandlerBaseContext? context = default, CancellationToken ct = default) where T : DXUnit, new();
        Task<JObject> InsertAsync(JObject jObject, DXHandlerBaseContext? context = default, CancellationToken ct = default);
        Task<JObject> UpdateAsync(JObject jObject, DXHandlerBaseContext? context = null, CancellationToken ct = default);
        Task<bool> DeleteAsync(JObject jObject, DXHandlerBaseContext? context = default, CancellationToken ct = default);
        Task<JObject> InsertOrUpdateAsync(JObject jObject, DXHandlerBaseContext? context = null, CancellationToken ct = default);

        Task<DXDataBlock<DXUnitRecord>> InsertAsync(DXDataBlock<DXUnitRecord> block, DXHandlerBaseContext? context = default, CancellationToken ct = default);
        Task<DXDataBlock<DXUnitRecord>> UpdateAsync(DXDataBlock<DXUnitRecord> block, DXHandlerBaseContext? context = null, CancellationToken ct = default);
        Task<bool> DeleteAsync(DXDataBlock<DXUnitRecord> block, DXHandlerBaseContext? context = default, CancellationToken ct = default);
        Task<DXDataBlock<DXUnitRecord>> InsertOrUpdateAsync(DXDataBlock<DXUnitRecord> block, DXHandlerBaseContext? context = null, CancellationToken ct = default);




        Task<bool> IsItemExistingAsync(string type, Guid id, DXHandlerBaseContext? context = default, CancellationToken ct = default);
    }
}
