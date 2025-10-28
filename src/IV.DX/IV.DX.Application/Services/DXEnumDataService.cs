using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Application.Services
{
    internal class DXEnumDataService(IDXEnumCoreRepository enumCoreRepo) : IDXEnumDataService
    {
        public async Task<IDictionary<int, string>> GetItemsAsync(string enumTypeName, CancellationToken ct = default)
        {
            var items = enumCoreRepo.GetItems(enumTypeName);

            return this.GetEnumValues(items);
        }

        private IDictionary<int, string> GetEnumValues(IEnumerable<DXModel> dxModel)
        {
            return dxModel?.ToDictionary(x => x.DXMainElement.Item.Content.Value<int>("Key"), x => x.DXMainElement.Item.Content.Value<string>("Value"));
        }
    }
}