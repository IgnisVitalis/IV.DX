using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Enums;
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
        Task<T> GetItemAsync<T>(Guid id, DXLoadingType typeOfLoading = DXLoadingType.Full, DXHandlerBaseContext? context = default, CancellationToken ct = default) where T : DXUnit, new();
        Task<IEnumerable<T>> GetItemsAsync<T>(DXHandlerBaseContext? context = default, DXLoadingType typeOfLoading = DXLoadingType.Full, CancellationToken ct = default) where T : DXUnit, new();
        Task<IEnumerable<T>> GetItemsAsync<T>(IEnumerable<Guid> ids, DXHandlerBaseContext? context = default, DXLoadingType typeOfLoading = DXLoadingType.Full, CancellationToken ct = default) where T : DXUnit, new();
        Task<IEnumerable<T>> GetItemsAsync<T>(string dxFilter, DXHandlerBaseContext? context = default, DXLoadingType typeOfLoading = DXLoadingType.Full, CancellationToken ct = default) where T : DXUnit, new();


        Task<JObject> GetItemAsync(string typeName, Guid id, DXHandlerBaseContext? context = default, CancellationToken ct = default);
        Task<IEnumerable<JObject>> GetItemsAsync(string typeName, DXHandlerBaseContext? context = default, CancellationToken ct = default);
        Task<IEnumerable<JObject>> GetItemsAsync(string typeName, IEnumerable<Guid> ids, DXHandlerBaseContext? context = default, CancellationToken ct = default);
        Task<IEnumerable<JObject>> GetItemsAsync(string typeName, string dxFilter, DXHandlerBaseContext? context = default, CancellationToken ct = default);
        Task<JObject> InsertAsync(JObject jObject, DXHandlerBaseContext? context = default, CancellationToken ct = default);
        Task<JObject> UpdateAsync(JObject jObject, DXHandlerBaseContext? context = null, CancellationToken ct = default);
        Task<bool> DeleteAsync(JObject jObject, DXHandlerBaseContext? context = default, CancellationToken ct = default);
        Task<JObject> InsertOrUpdateAsync(JObject jObject, DXHandlerBaseContext? context = null, CancellationToken ct = default);




        Task<bool> IsItemExistingAsync(string type, Guid id, DXHandlerBaseContext? context = default, CancellationToken ct = default);
    }
}