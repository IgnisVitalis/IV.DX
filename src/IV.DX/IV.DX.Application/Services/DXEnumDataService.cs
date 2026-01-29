using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Application.Services
{
    internal class DXEnumDataService(
        IDXStructureCache dxStructureCache,
        IDXRawReader dxRawReader,
        IDXEnumCoreRepository enumCoreRepository) : IDXEnumDataService
    {
        public async Task<IDictionary<int, string>> GetItemsAsync(string enumTypeName, CancellationToken ct = default)
        {
            var existingDXEnum = dxStructureCache.GetDXEnum(enumTypeName);

            if (existingDXEnum == null)
                throw new Exception($"There are no DXEnum with name '{enumTypeName}'");

            var keyColumn = existingDXEnum.GetColumnValue();
            var valueExpression = existingDXEnum.DisplayValue;

            var columns = new Dictionary<string, string>()
            {
                {"Key",  keyColumn.Name  },
                {"Value", valueExpression }
            };

            var enums = dxRawReader.Get(enumTypeName, columns);

            var items = enums.Announced.ToDictionary(x => x.GetValue<int>("Key"), x => x.GetValue<string>("Value"));

            return items;
        }

        public async Task<DXDataBlock<DXEnumRecord>> InsertOrUpdateAsync(DXDataBlock<DXEnumRecord> block, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(block);

            enumCoreRepository.InsertOrUpdate(block);

            return block;
        }
    }
}
