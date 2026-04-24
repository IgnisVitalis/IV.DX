using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Application.Contracts.Abstractions
{
    public interface IDXUnitDataReader
    {
        Task<T?> GetItemAsync<T>(Guid id, DXLoadingType typeOfLoading = DXLoadingType.Full, DXHandlerBaseContext? context = default, CancellationToken ct = default) where T : DXUnit, new();
        Task<IEnumerable<T>> GetItemsAsync<T>(DXHandlerBaseContext? context = default, DXLoadingType typeOfLoading = DXLoadingType.Full, CancellationToken ct = default) where T : DXUnit, new();
        Task<IEnumerable<T>> GetItemsAsync<T>(IEnumerable<Guid> ids, DXHandlerBaseContext? context = default, DXLoadingType typeOfLoading = DXLoadingType.Full, CancellationToken ct = default) where T : DXUnit, new();
        Task<IEnumerable<T>> GetItemsAsync<T>(string dxFilter, DXHandlerBaseContext? context = default, DXLoadingType typeOfLoading = DXLoadingType.Full, CancellationToken ct = default) where T : DXUnit, new();

        Task<JObject?> GetItemAsync(string typeName, Guid id, DXHandlerBaseContext? context = default, CancellationToken ct = default);
        Task<JObject> GetItemsAsync(string typeName, DXHandlerBaseContext? context = default, CancellationToken ct = default);
        Task<JObject> GetItemsAsync(string typeName, IEnumerable<Guid> ids, DXHandlerBaseContext? context = default, CancellationToken ct = default);
        Task<JObject> GetItemsAsync(string typeName, string dxFilter, DXHandlerBaseContext? context = default, CancellationToken ct = default);

        Task<JObject?> GetItemAsync(Guid unitDefinitionId, Guid id, DXHandlerBaseContext? context = default, CancellationToken ct = default);
        Task<JObject> GetItemsAsync(Guid unitDefinitionId, DXHandlerBaseContext? context = default, CancellationToken ct = default);
        Task<JObject> GetItemsAsync(Guid unitDefinitionId, IEnumerable<Guid> ids, DXHandlerBaseContext? context = default, CancellationToken ct = default);
        Task<JObject> GetItemsAsync(Guid unitDefinitionId, string dxFilter, DXHandlerBaseContext? context = default, CancellationToken ct = default);
    }
}
