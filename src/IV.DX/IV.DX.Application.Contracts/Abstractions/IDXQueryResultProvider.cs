using IV.DX.Application.Contracts.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Application.Contracts.Abstractions
{
    public interface IDXQueryResultProvider
    {
        Task<JObject> GetAsync(Guid dxQueryID, Guid? dxFilterID, CancellationToken ct = default);

        Task<IEnumerable<DXDisplayValue>> GetDisplayValuesAsync(string typeName, CancellationToken ct = default);
    }
}