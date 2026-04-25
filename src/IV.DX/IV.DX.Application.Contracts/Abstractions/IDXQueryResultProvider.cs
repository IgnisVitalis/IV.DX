using IV.DX.Application.Contracts.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Application.Contracts.Abstractions
{
    public interface IDXQueryResultProvider
    {
        Task<JObject?> GetAsync(Guid dxQueryId, CancellationToken ct = default);

        Task<IEnumerable<DXTitleExpression>> GetDXTitleExpressionsAsync(string typeName, CancellationToken ct = default);
    }
}