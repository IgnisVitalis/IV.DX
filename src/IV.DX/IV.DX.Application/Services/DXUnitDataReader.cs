using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.Pipeline;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Newtonsoft.Json.Linq;

namespace IV.DX.Application.Services
{
    internal sealed class DXUnitDataReader(IDXPipelineExecutor dxPipelineExecutor, IDXStructureCache structureCache) : IDXUnitDataReader
    {
        public async Task<T> GetItemAsync<T>(Guid id, DXLoadingType typeOfLoading = DXLoadingType.Full, DXHandlerBaseContext? context = default, CancellationToken ct = default) where T : DXUnit, new()
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            var result = await dxPipelineExecutor.GetAsync<T>(id, context, ct);

            if (result.IsSuccess)
            {
                if (result.Outcome == DXOutcome.Ok && result.Value != null)
                {
                    return result.Value;
                }
                else if (result.Outcome == DXOutcome.NotFound)
                {
                    return null;
                }
            }

            throw new Exception($"There are an error to get dxUnit by ID ({id}): {result.Error}");
        }

        public async Task<IEnumerable<T>> GetItemsAsync<T>(DXHandlerBaseContext? context = default, DXLoadingType typeOfLoading = DXLoadingType.Full, CancellationToken ct = default) where T : DXUnit, new()
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            var result = await dxPipelineExecutor.GetItemsAsync<T>(context, ct);

            if (result.IsSuccess)
            {
                if (result.Outcome == DXOutcome.Ok && result.Value != null)
                {
                    return result.Value;
                }
                else if (result.Outcome == DXOutcome.NotFound)
                {
                    return Enumerable.Empty<T>();
                }
            }

            throw new Exception($"There are an error to get all dxUnit: {result.Error}");
        }

        public async Task<IEnumerable<T>> GetItemsAsync<T>(IEnumerable<Guid> ids, DXHandlerBaseContext? context = default, DXLoadingType typeOfLoading = DXLoadingType.Full, CancellationToken ct = default) where T : DXUnit, new()
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            var result = await dxPipelineExecutor.GetItemsAsync<T>(ids, context, ct);

            if (result.IsSuccess)
            {
                if (result.Outcome == DXOutcome.Ok && result.Value != null)
                {
                    return result.Value;
                }
                else if (result.Outcome == DXOutcome.NotFound)
                {
                    return Enumerable.Empty<T>();
                }
            }

            throw new Exception($"There are an error to get dxUnit by ids: {result.Error}");
        }

        public async Task<IEnumerable<T>> GetItemsAsync<T>(string dxFilter, DXHandlerBaseContext? context = default, DXLoadingType typeOfLoading = DXLoadingType.Full, CancellationToken ct = default) where T : DXUnit, new()
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            var result = await dxPipelineExecutor.GetItemsAsync<T>(dxFilter, context, ct);

            if (result.IsSuccess)
            {
                if (result.Outcome == DXOutcome.Ok && result.Value != null)
                {
                    return result.Value;
                }
                else if (result.Outcome == DXOutcome.NotFound)
                {
                    return Enumerable.Empty<T>();
                }
            }

            throw new Exception($"There are an error to get dxUnit by query ({dxFilter}): {result.Error}");
        }

        public async Task<JObject> GetItemAsync(string typeName, Guid id, DXHandlerBaseContext? context = default, CancellationToken ct = default)
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            var result = await dxPipelineExecutor.GetAsync(typeName, id, context, ct);

            if (result.IsSuccess)
            {
                if (result.Outcome == DXOutcome.Ok && result.Value != null)
                {
                    return MaskSensitive(result.Value, typeName);
                }
                else if (result.Outcome == DXOutcome.NotFound)
                {
                    return null;
                }
            }

            throw new Exception($"There are an error to get dxModel by ID ({id}): {result.Error}");
        }

        public async Task<IEnumerable<JObject>> GetItemsAsync(string typeName, DXHandlerBaseContext? context = default, CancellationToken ct = default)
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            var result = await dxPipelineExecutor.GetItemsAsync(typeName, context, ct);

            if (result.IsSuccess)
            {
                if (result.Outcome == DXOutcome.Ok && result.Value != null)
                {
                    return result.Value.Select(x => MaskSensitive(x, typeName)).ToList();
                }
                else if (result.Outcome == DXOutcome.NotFound || result.Value == null)
                {
                    return Enumerable.Empty<JObject>();
                }
            }

            throw new Exception($"There are an error to get all dxModel by type ({typeName}): {result.Error}");
        }

        public async Task<IEnumerable<JObject>> GetItemsAsync(string typeName, IEnumerable<Guid> ids, DXHandlerBaseContext? context = default, CancellationToken ct = default)
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            var result = await dxPipelineExecutor.GetItemsAsync(typeName, ids, context, ct);

            if (result.IsSuccess)
            {
                if (result.Outcome == DXOutcome.Ok && result.Value != null)
                {
                    return result.Value.Select(x => MaskSensitive(x, typeName)).ToList();
                }
                else if (result.Outcome == DXOutcome.NotFound || result.Value == null)
                {
                    return Enumerable.Empty<JObject>();
                }
            }

            throw new Exception($"There are an error to get all dxModel by type ({typeName}) and IDs: {result.Error}");
        }

        public async Task<IEnumerable<JObject>> GetItemsAsync(string typeName, string dxFilter, DXHandlerBaseContext? context = default, CancellationToken ct = default)
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            var result = await dxPipelineExecutor.GetItemsAsync(typeName, dxFilter, context, ct);

            if (result.IsSuccess)
            {
                if (result.Outcome == DXOutcome.Ok && result.Value != null)
                {
                    return result.Value.Select(x => MaskSensitive(x, typeName)).ToList();
                }
                else if (result.Outcome == DXOutcome.NotFound || result.Value == null)
                {
                    return Enumerable.Empty<JObject>();
                }
            }

            throw new Exception($"There are an error to get all dxModel by type ({typeName}) and query ({dxFilter}): {result.Error}");
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
