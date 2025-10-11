using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Application.Contracts.Abstractions
{
    public interface IDXUnitDataService
    {
        Task<T> InsertAsync<T>(T esqlObject, IDXHandlerContext? context = default, CancellationToken ct = default) where T : DXUnit, new();
        Task<T> UpdateAsync<T>(T esqlObject, IDXHandlerContext? context = default, CancellationToken ct = default) where T : DXUnit, new();
        Task<T> InsertOrUpdateAsync<T>(T esqlObject, IDXHandlerContext? context = default, CancellationToken ct = default) where T : DXUnit, new();
        Task<bool> DeleteAsync<T>(T esqlObject, IDXHandlerContext? context = default, CancellationToken ct = default) where T : DXUnit, new(); 
        Task<T> GetItemAsync<T>(Guid id, DXLoadingType typeOfLoading = DXLoadingType.Full, IDXHandlerContext? context = default, CancellationToken ct = default) where T : DXUnit, new();
        Task<IEnumerable<T>> GetItemsAsync<T>(IDXHandlerContext? context = default, DXLoadingType typeOfLoading = DXLoadingType.Full, CancellationToken ct = default) where T : DXUnit, new();
        Task<IEnumerable<T>> GetItemsAsync<T>(IEnumerable<Guid> ids, IDXHandlerContext? context = default, DXLoadingType typeOfLoading = DXLoadingType.Full, CancellationToken ct = default) where T : DXUnit, new();
        Task<IEnumerable<T>> GetItemsAsync<T>(string esqlWhereExpression, IDXHandlerContext? context = default, DXLoadingType typeOfLoading = DXLoadingType.Full, CancellationToken ct = default) where T : DXUnit, new();


        Task<JObject> GetItemAsync(string typeName, Guid id, IDXHandlerContext? context = default, CancellationToken ct = default);
        Task<IEnumerable<JObject>> GetItemsAsync(string typeName, IDXHandlerContext? context = default, CancellationToken ct = default);
        Task<IEnumerable<JObject>> GetItemsAsync(string typeName, IEnumerable<Guid> ids, IDXHandlerContext? context = default, CancellationToken ct = default);
        Task<IEnumerable<JObject>> GetItemsAsync(string typeName, string esqlWhereExpression, IDXHandlerContext? context = default, CancellationToken ct = default);
        Task<JObject> InsertAsync(JObject jObject, IDXHandlerContext? context = default, CancellationToken ct = default);
        Task<JObject> UpdateAsync(JObject jObject, IDXHandlerContext? context = null, CancellationToken ct = default);
        Task<bool> DeleteAsync(JObject jObject, IDXHandlerContext? context = default, CancellationToken ct = default);
        Task<JObject> InsertOrUpdateAsync(JObject jObject, IDXHandlerContext? context = null, CancellationToken ct = default);




        Task<bool> IsItemExistingAsync(Guid id, string type, IDXHandlerContext? context = default, CancellationToken ct = default);
    }
}