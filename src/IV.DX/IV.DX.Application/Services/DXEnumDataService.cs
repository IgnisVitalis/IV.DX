using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using System.Collections.Generic;

namespace IV.DX.Application.Services
{
    internal class DXEnumDataService(
        IDXStructureCache dxStructureCache,
        IDXRawReader dxRawReader,
        IDXEnumCoreRepository enumCoreRepository) : IDXEnumDataService
    {
        public Task<IDictionary<int, string>> GetItemsAsync(string enumTypeName, CancellationToken ct = default)
        {
            var existingDXEnum = dxStructureCache.GetDXEnum(enumTypeName);

            if (existingDXEnum == null)
                throw new Exception($"There are no DXEnum with name '{enumTypeName}'");

            var keyColumn = existingDXEnum.GetColumnValue();
            var valueExpression = existingDXEnum.DXTitleExpression;

            var columns = new Dictionary<string, string>()
            {
                {"Key",  keyColumn.Name  },
                {"Value", valueExpression }
            };

            var enums = dxRawReader.Get(enumTypeName, columns);
            var records = enums.Data?.Items ?? new List<DXUnitRecord>();

            var items = records.ToDictionary(
                x => x.Fields != null && x.Fields.TryGetValue("Key", out var k)
                    ? k.ToObject<int>()
                    : default,
                x => x.Fields != null && x.Fields.TryGetValue("Value", out var v)
                    ? v.ToString()
                    : string.Empty);

            return Task.FromResult<IDictionary<int, string>>(items);
        }

        public Task<DXDataBlock<DXEnumRecord>> InsertOrUpdateAsync(DXDataBlock<DXEnumRecord> block, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(block);

            enumCoreRepository.InsertOrUpdate(block);

            return Task.FromResult(block);
        }
    }
}

