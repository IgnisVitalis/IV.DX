using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Newtonsoft.Json.Linq;

namespace IV.DX.Application.Services
{
    internal sealed class DXUnitDataReader(IDXUnitDataService dataService, IDXStructureCache structureCache) : IDXUnitDataReader
    {
        public async Task<JObject> GetItemAsync(string typeName, Guid id, DXHandlerBaseContext? context = default, CancellationToken ct = default)
        {
            var obj = await dataService.GetItemAsync(typeName, id, context, ct);
            return MaskSensitive(obj, typeName);
        }

        public async Task<IEnumerable<JObject>> GetItemsAsync(string typeName, DXHandlerBaseContext? context = default, CancellationToken ct = default)
        {
            var items = await dataService.GetItemsAsync(typeName, context, ct);
            return items.Select(x => MaskSensitive(x, typeName)).ToList();
        }

        public async Task<IEnumerable<JObject>> GetItemsAsync(string typeName, IEnumerable<Guid> ids, DXHandlerBaseContext? context = default, CancellationToken ct = default)
        {
            var items = await dataService.GetItemsAsync(typeName, ids, context, ct);
            return items.Select(x => MaskSensitive(x, typeName)).ToList();
        }

        public async Task<IEnumerable<JObject>> GetItemsAsync(string typeName, string dxFilter, DXHandlerBaseContext? context = default, CancellationToken ct = default)
        {
            var items = await dataService.GetItemsAsync(typeName, dxFilter, context, ct);
            return items.Select(x => MaskSensitive(x, typeName)).ToList();
        }

        private JObject MaskSensitive(JObject? jObject, string? typeName)
        {
            if (jObject == null)
                return null;

            var block = jObject.ToObject<DXDataBlock<DXUnitRecord>>();
            if (block == null)
                return jObject;

            MaskBlock(block, typeName);
            return JObject.FromObject(block);
        }

        private void MaskBlock(DXDataBlock<DXUnitRecord> block, string? unitTypeName)
        {
            var typeName = !string.IsNullOrWhiteSpace(unitTypeName) ? unitTypeName : block.Meta?.Type;
            if (string.IsNullOrWhiteSpace(typeName))
                return;

            var unitSensitive = GetSensitiveColumnsForUnit(typeName);

            var items = block.Data?.Items;
            if (items == null) return;

            foreach (var record in items)
            {
                if (record?.Fields != null)
                {
                    foreach (var col in unitSensitive)
                    {
                        if (record.Fields.TryGetValue(col, out var token) && token != null && token.Type != JTokenType.Null)
                        {
                            record.Fields[col] = JToken.FromObject(string.Empty);
                        }
                    }
                }

                if (record?.DXElements == null) continue;

                foreach (var elementBlock in record.DXElements.Values)
                {
                    var elementType = elementBlock?.Meta?.Type;
                    if (string.IsNullOrWhiteSpace(elementType))
                        continue;

                    var elementSensitive = GetSensitiveColumnsForElement(elementType);
                    if (elementSensitive.Count == 0)
                        continue;

                    var elItems = elementBlock?.Data?.Items;
                    if (elItems == null) continue;

                    foreach (var el in elItems)
                    {
                        if (el?.Fields == null) continue;

                        foreach (var col in elementSensitive)
                        {
                            if (el.Fields.TryGetValue(col, out var token) && token != null && token.Type != JTokenType.Null)
                            {
                                el.Fields[col] = JToken.FromObject(string.Empty);
                            }
                        }
                    }
                }
            }
        }

        private HashSet<string> GetSensitiveColumnsForUnit(string unitTypeName)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var unit = structureCache.GetDXUnit(unitTypeName);
            if (unit == null)
                return result;

            var hierarchy = structureCache.GetDXUnitInheritance(unit);
            foreach (var item in hierarchy.Items)
            {
                var columns = item.DXUnit?.DXColumnDefinitionElement?.Announced;
                if (columns == null) continue;

                foreach (var c in columns)
                {
                    if (c == null) continue;
                    if (c.ColumnType == DXColumnTypeEnum.HashedString || c.ColumnType == DXColumnTypeEnum.EncryptedString)
                        result.Add(c.Name);
                }
            }

            return result;
        }

        private HashSet<string> GetSensitiveColumnsForElement(string elementTypeName)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var el = structureCache.GetDXElement(elementTypeName);
            if (el == null)
                return result;

            var columns = el.DXColumnDefinitionElement?.Announced;
            if (columns == null) return result;

            foreach (var c in columns)
            {
                if (c == null) continue;
                if (c.ColumnType == DXColumnTypeEnum.HashedString || c.ColumnType == DXColumnTypeEnum.EncryptedString)
                    result.Add(c.Name);
            }

            return result;
        }
    }
}
