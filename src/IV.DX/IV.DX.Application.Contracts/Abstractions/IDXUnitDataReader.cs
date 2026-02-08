using IV.DX.Application.Contracts.Runtime;
using Newtonsoft.Json.Linq;

namespace IV.DX.Application.Contracts.Abstractions
{
    public interface IDXUnitDataReader
    {
        Task<JObject> GetItemAsync(string typeName, Guid id, DXHandlerBaseContext? context = default, CancellationToken ct = default);
        Task<IEnumerable<JObject>> GetItemsAsync(string typeName, DXHandlerBaseContext? context = default, CancellationToken ct = default);
        Task<IEnumerable<JObject>> GetItemsAsync(string typeName, IEnumerable<Guid> ids, DXHandlerBaseContext? context = default, CancellationToken ct = default);
        Task<IEnumerable<JObject>> GetItemsAsync(string typeName, string dxFilter, DXHandlerBaseContext? context = default, CancellationToken ct = default);
    }
}

