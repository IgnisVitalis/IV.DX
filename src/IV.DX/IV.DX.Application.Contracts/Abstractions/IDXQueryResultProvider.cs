using Newtonsoft.Json.Linq;

namespace IV.DX.Application.Contracts.Abstractions
{
    public interface IDXQueryResultProvider
    {
        Task<JObject> GetAsync(Guid dxQueryID, CancellationToken ct = default);
    }
}
