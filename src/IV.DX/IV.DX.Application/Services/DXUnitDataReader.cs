using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.Pipeline;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace IV.DX.Application.Services
{
    internal sealed class DXUnitDataReader(
        IDXPipelineExecutor dxPipelineExecutor,
        IDXStructureCache structureCache,
        IDXUnitTypeAccessChecker unitTypeAccessChecker,
        IDXUnitGenericRepository genericRepo,
        IDXExecutionContextAccessor executionContextAccessor,
        ILogger<DXUnitDataReader> logger) : IDXUnitDataReader
    {
        public async Task<T?> GetItemAsync<T>(Guid id, DXLoadingType typeOfLoading = DXLoadingType.Full, DXHandlerBaseContext? context = default, CancellationToken ct = default) where T : DXUnit, new()
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            var typeName = AttributeReader.GetDXUnitTypeName(typeof(T));
            var decision = unitTypeAccessChecker.CheckAccess(typeName, DXUnitTypeAccessOperation.Read);
            var publicFallback = ShouldApplyAnonymousPublicFallback(decision);

            if (decision == DXAccessDecision.Denied && !publicFallback)
                ThrowDenied(typeName, DXUnitTypeAccessOperation.Read);

            if ((decision == DXAccessDecision.AllowedOwnedOnly || publicFallback) && !IsReadOwned(typeName, id))
                return null;

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

            LogReadFailure("GetItem", typeName, result.Error, id: id);
            throw new Exception($"There are an error to get dxUnit by ID ({id}): {result.Error}");
        }

        public async Task<IEnumerable<T>> GetItemsAsync<T>(DXHandlerBaseContext? context = default, DXLoadingType typeOfLoading = DXLoadingType.Full, CancellationToken ct = default) where T : DXUnit, new()
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            var typeName = AttributeReader.GetDXUnitTypeName(typeof(T));
            var decision = unitTypeAccessChecker.CheckAccess(typeName, DXUnitTypeAccessOperation.Read);
            var publicFallback = ShouldApplyAnonymousPublicFallback(decision);

            if (decision == DXAccessDecision.Denied && !publicFallback)
                ThrowDenied(typeName, DXUnitTypeAccessOperation.Read);

            if (decision == DXAccessDecision.AllowedOwnedOnly || publicFallback)
            {
                var ctx = executionContextAccessor.Current;
                var ownedIds = CollectOwnedIds(typeName, ctx);
                if (ownedIds.Count == 0)
                    return Enumerable.Empty<T>();

                var idFilter = BuildIdInFilter(ownedIds, null);
                var filteredResult = await dxPipelineExecutor.GetItemsAsync<T>(idFilter, context, ct);

                if (filteredResult.IsSuccess)
                {
                    if (filteredResult.Outcome == DXOutcome.Ok && filteredResult.Value != null)
                        return filteredResult.Value;
                    if (filteredResult.Outcome == DXOutcome.NotFound)
                        return Enumerable.Empty<T>();
                }

                LogReadFailure("GetItems", typeName, filteredResult.Error);
                throw new Exception($"There are an error to get all dxUnit: {filteredResult.Error}");
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

            LogReadFailure("GetItems", typeName, result.Error);
            throw new Exception($"There are an error to get all dxUnit: {result.Error}");
        }

        public async Task<IEnumerable<T>> GetItemsAsync<T>(IEnumerable<Guid> ids, DXHandlerBaseContext? context = default, DXLoadingType typeOfLoading = DXLoadingType.Full, CancellationToken ct = default) where T : DXUnit, new()
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            var typeName = AttributeReader.GetDXUnitTypeName(typeof(T));
            var decision = unitTypeAccessChecker.CheckAccess(typeName, DXUnitTypeAccessOperation.Read);
            var publicFallback = ShouldApplyAnonymousPublicFallback(decision);

            if (decision == DXAccessDecision.Denied && !publicFallback)
                ThrowDenied(typeName, DXUnitTypeAccessOperation.Read);

            if (decision == DXAccessDecision.AllowedOwnedOnly || publicFallback)
            {
                var ctx = executionContextAccessor.Current;
                var ownedIds = CollectOwnedIds(typeName, ctx);
                var filteredIds = ids.Where(id => ownedIds.Contains(id)).ToList();

                if (filteredIds.Count == 0)
                    return Enumerable.Empty<T>();

                var filteredResult = await dxPipelineExecutor.GetItemsAsync<T>(filteredIds, context, ct);

                if (filteredResult.IsSuccess)
                {
                    if (filteredResult.Outcome == DXOutcome.Ok && filteredResult.Value != null)
                        return filteredResult.Value;
                    if (filteredResult.Outcome == DXOutcome.NotFound)
                        return Enumerable.Empty<T>();
                }

                LogReadFailure("GetItemsByIds", typeName, filteredResult.Error);
                throw new Exception($"There are an error to get dxUnit by ids: {filteredResult.Error}");
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

            LogReadFailure("GetItemsByIds", typeName, result.Error);
            throw new Exception($"There are an error to get dxUnit by ids: {result.Error}");
        }

        public async Task<IEnumerable<T>> GetItemsAsync<T>(string dxFilter, DXHandlerBaseContext? context = default, DXLoadingType typeOfLoading = DXLoadingType.Full, CancellationToken ct = default) where T : DXUnit, new()
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            var typeName = AttributeReader.GetDXUnitTypeName(typeof(T));
            var decision = unitTypeAccessChecker.CheckAccess(typeName, DXUnitTypeAccessOperation.Read);
            var publicFallback = ShouldApplyAnonymousPublicFallback(decision);

            if (decision == DXAccessDecision.Denied && !publicFallback)
                ThrowDenied(typeName, DXUnitTypeAccessOperation.Read);

            if (decision == DXAccessDecision.AllowedOwnedOnly || publicFallback)
            {
                var ctx = executionContextAccessor.Current;
                var ownedIds = CollectOwnedIds(typeName, ctx);
                if (ownedIds.Count == 0)
                    return Enumerable.Empty<T>();

                var composedFilter = BuildIdInFilter(ownedIds, dxFilter);
                var filteredResult = await dxPipelineExecutor.GetItemsAsync<T>(composedFilter, context, ct);

                if (filteredResult.IsSuccess)
                {
                    if (filteredResult.Outcome == DXOutcome.Ok && filteredResult.Value != null)
                        return filteredResult.Value;
                    if (filteredResult.Outcome == DXOutcome.NotFound)
                        return Enumerable.Empty<T>();
                }

                LogReadFailure("GetItemsByFilter", typeName, filteredResult.Error, filter: dxFilter);
                throw new Exception($"There are an error to get dxUnit by query ({dxFilter}): {filteredResult.Error}");
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

            LogReadFailure("GetItemsByFilter", typeName, result.Error, filter: dxFilter);
            throw new Exception($"There are an error to get dxUnit by query ({dxFilter}): {result.Error}");
        }

        public async Task<JObject?> GetItemAsync(string typeName, Guid id, DXHandlerBaseContext? context = default, CancellationToken ct = default)
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            var decision = unitTypeAccessChecker.CheckAccess(typeName, DXUnitTypeAccessOperation.Read);
            var publicFallback = ShouldApplyAnonymousPublicFallback(decision);

            if (decision == DXAccessDecision.Denied && !publicFallback)
                ThrowDenied(typeName, DXUnitTypeAccessOperation.Read);

            if ((decision == DXAccessDecision.AllowedOwnedOnly || publicFallback) && !IsReadOwned(typeName, id))
                return null;

            var result = await dxPipelineExecutor.GetAsync(typeName, id, context, ct);

            if (result.IsSuccess)
            {
                if (result.Outcome == DXOutcome.Ok && result.Value != null)
                {
                    return MaskSensitive(result.Value, typeName)!;
                }
                else if (result.Outcome == DXOutcome.NotFound)
                {
                    return null;
                }
            }

            LogReadFailure("GetItem", typeName, result.Error, id: id);
            throw new Exception($"There are an error to get dxModel by ID ({id}): {result.Error}");
        }

        public async Task<JObject> GetItemsAsync(string typeName, DXHandlerBaseContext? context = default, CancellationToken ct = default)
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            var decision = unitTypeAccessChecker.CheckAccess(typeName, DXUnitTypeAccessOperation.Read);
            var publicFallback = ShouldApplyAnonymousPublicFallback(decision);

            if (decision == DXAccessDecision.Denied && !publicFallback)
                ThrowDenied(typeName, DXUnitTypeAccessOperation.Read);

            if (decision == DXAccessDecision.AllowedOwnedOnly || publicFallback)
            {
                var ctx = executionContextAccessor.Current;
                var ownedIds = CollectOwnedIds(typeName, ctx);
                if (ownedIds.Count == 0)
                    return EmptyDXFormat(typeName);

                var idFilter = BuildIdInFilter(ownedIds, null);
                var filteredResult = await dxPipelineExecutor.GetItemsAsync(typeName, idFilter, context, ct);

                if (filteredResult.IsSuccess)
                {
                    if (filteredResult.Outcome == DXOutcome.Ok && filteredResult.Value != null)
                        return MaskSensitive(filteredResult.Value, typeName)!;
                    if (filteredResult.Outcome == DXOutcome.NotFound || filteredResult.Value == null)
                        return EmptyDXFormat(typeName);
                }

                LogReadFailure("GetItems", typeName, filteredResult.Error);
                throw new Exception($"There are an error to get all dxModel by type ({typeName}): {filteredResult.Error}");
            }

            var result = await dxPipelineExecutor.GetItemsAsync(typeName, context, ct);

            if (result.IsSuccess)
            {
                if (result.Outcome == DXOutcome.Ok && result.Value != null)
                    return MaskSensitive(result.Value, typeName)!;
                if (result.Outcome == DXOutcome.NotFound || result.Value == null)
                    return EmptyDXFormat(typeName);
            }

            LogReadFailure("GetItems", typeName, result.Error);
            throw new Exception($"There are an error to get all dxModel by type ({typeName}): {result.Error}");
        }

        public async Task<JObject> GetItemsAsync(string typeName, IEnumerable<Guid> ids, DXHandlerBaseContext? context = default, CancellationToken ct = default)
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            var decision = unitTypeAccessChecker.CheckAccess(typeName, DXUnitTypeAccessOperation.Read);
            var publicFallback = ShouldApplyAnonymousPublicFallback(decision);

            if (decision == DXAccessDecision.Denied && !publicFallback)
                ThrowDenied(typeName, DXUnitTypeAccessOperation.Read);

            if (decision == DXAccessDecision.AllowedOwnedOnly || publicFallback)
            {
                var ctx = executionContextAccessor.Current;
                var ownedIds = CollectOwnedIds(typeName, ctx);
                var filteredIds = ids.Where(id => ownedIds.Contains(id)).ToList();

                if (filteredIds.Count == 0)
                    return EmptyDXFormat(typeName);

                var filteredResult = await dxPipelineExecutor.GetItemsAsync(typeName, filteredIds, context, ct);

                if (filteredResult.IsSuccess)
                {
                    if (filteredResult.Outcome == DXOutcome.Ok && filteredResult.Value != null)
                        return MaskSensitive(filteredResult.Value, typeName)!;
                    if (filteredResult.Outcome == DXOutcome.NotFound || filteredResult.Value == null)
                        return EmptyDXFormat(typeName);
                }

                LogReadFailure("GetItemsByIds", typeName, filteredResult.Error);
                throw new Exception($"There are an error to get all dxModel by type ({typeName}) and IDs: {filteredResult.Error}");
            }

            var result = await dxPipelineExecutor.GetItemsAsync(typeName, ids, context, ct);

            if (result.IsSuccess)
            {
                if (result.Outcome == DXOutcome.Ok && result.Value != null)
                    return MaskSensitive(result.Value, typeName)!;
                if (result.Outcome == DXOutcome.NotFound || result.Value == null)
                    return EmptyDXFormat(typeName);
            }

            LogReadFailure("GetItemsByIds", typeName, result.Error);
            throw new Exception($"There are an error to get all dxModel by type ({typeName}) and IDs: {result.Error}");
        }

        public async Task<JObject> GetItemsAsync(string typeName, string dxFilter, DXHandlerBaseContext? context = default, CancellationToken ct = default)
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            var decision = unitTypeAccessChecker.CheckAccess(typeName, DXUnitTypeAccessOperation.Read);
            var publicFallback = ShouldApplyAnonymousPublicFallback(decision);

            if (decision == DXAccessDecision.Denied && !publicFallback)
                ThrowDenied(typeName, DXUnitTypeAccessOperation.Read);

            if (decision == DXAccessDecision.AllowedOwnedOnly || publicFallback)
            {
                var ctx = executionContextAccessor.Current;
                var ownedIds = CollectOwnedIds(typeName, ctx);
                if (ownedIds.Count == 0)
                    return EmptyDXFormat(typeName);

                var composedFilter = BuildIdInFilter(ownedIds, dxFilter);
                var filteredResult = await dxPipelineExecutor.GetItemsAsync(typeName, composedFilter, context, ct);

                if (filteredResult.IsSuccess)
                {
                    if (filteredResult.Outcome == DXOutcome.Ok && filteredResult.Value != null)
                        return MaskSensitive(filteredResult.Value, typeName)!;
                    if (filteredResult.Outcome == DXOutcome.NotFound || filteredResult.Value == null)
                        return EmptyDXFormat(typeName);
                }

                LogReadFailure("GetItemsByFilter", typeName, filteredResult.Error, filter: dxFilter);
                throw new Exception($"There are an error to get all dxModel by type ({typeName}) and query ({dxFilter}): {filteredResult.Error}");
            }

            var result = await dxPipelineExecutor.GetItemsAsync(typeName, dxFilter, context, ct);

            if (result.IsSuccess)
            {
                if (result.Outcome == DXOutcome.Ok && result.Value != null)
                    return MaskSensitive(result.Value, typeName)!;
                if (result.Outcome == DXOutcome.NotFound || result.Value == null)
                    return EmptyDXFormat(typeName);
            }

            LogReadFailure("GetItemsByFilter", typeName, result.Error, filter: dxFilter);
            throw new Exception($"There are an error to get all dxModel by type ({typeName}) and query ({dxFilter}): {result.Error}");
        }

        public Task<JObject?> GetItemAsync(Guid unitDefinitionId, Guid id, DXHandlerBaseContext? context = default, CancellationToken ct = default)
            => GetItemAsync(ResolveTypeName(unitDefinitionId), id, context, ct);

        public Task<JObject> GetItemsAsync(Guid unitDefinitionId, DXHandlerBaseContext? context = default, CancellationToken ct = default)
            => GetItemsAsync(ResolveTypeName(unitDefinitionId), context, ct);

        public Task<JObject> GetItemsAsync(Guid unitDefinitionId, IEnumerable<Guid> ids, DXHandlerBaseContext? context = default, CancellationToken ct = default)
            => GetItemsAsync(ResolveTypeName(unitDefinitionId), ids, context, ct);

        public Task<JObject> GetItemsAsync(Guid unitDefinitionId, string dxFilter, DXHandlerBaseContext? context = default, CancellationToken ct = default)
            => GetItemsAsync(ResolveTypeName(unitDefinitionId), dxFilter, context, ct);

        private string ResolveTypeName(Guid unitDefinitionId)
        {
            var unit = structureCache.DXUnits.FirstOrDefault(x => x.ID == unitDefinitionId)
                ?? throw new ArgumentException($"No DX unit definition found for ID '{unitDefinitionId}'.");
            return unit.Name;
        }

        private bool IsReadOwned(string typeName, Guid instanceId)
        {
            var ctx = executionContextAccessor.Current;

            var unitDef = structureCache.GetDXUnit(typeName);
            if (unitDef == null)
                return false;

            if (unitDef.SupportsOwnership && ctx?.IdentityID.HasValue == true)
            {
                var identityOwnership = genericRepo
                    .GetDXUnits<DXIdentityOwnershipUnit>(
                        $"Identity = '{ctx.IdentityID.Value}' AND DXUnitDefinition = '{unitDef.ID}' AND OwnedDXUnitID = '{instanceId}'")
                    .FirstOrDefault();

                if (identityOwnership != null)
                    return true;
            }

            if (unitDef.SupportsOwnership && ctx?.ActiveGroupIDs != null)
            {
                foreach (var groupId in ctx.ActiveGroupIDs)
                {
                    var groupOwnership = genericRepo
                        .GetDXUnits<DXGroupOwnershipUnit>(
                            $"Group = '{groupId}' AND DXUnitDefinition = '{unitDef.ID}' AND OwnedDXUnitID = '{instanceId}'")
                        .FirstOrDefault();

                    if (groupOwnership != null)
                        return true;
                }
            }

            var publicAccess = genericRepo
                .GetDXUnits<DXPublicAccessUnit>(
                    $"DXUnitDefinition = '{unitDef.ID}' AND PublicDXUnitID = '{instanceId}'")
                .FirstOrDefault();

            return publicAccess != null;
        }

        private HashSet<Guid> CollectOwnedIds(string typeName, DXExecutionContext? ctx)
        {
            var result = new HashSet<Guid>();

            var unitDef = structureCache.GetDXUnit(typeName);
            if (unitDef == null)
                return result;

            if (unitDef.SupportsOwnership && ctx?.IdentityID.HasValue == true)
            {
                var identityOwned = genericRepo.GetDXUnits<DXIdentityOwnershipUnit>(
                    $"Identity = '{ctx.IdentityID.Value}' AND DXUnitDefinition = '{unitDef.ID}'");

                foreach (var o in identityOwned)
                    result.Add(o.OwnedDXUnitID);
            }

            if (unitDef.SupportsOwnership && ctx?.ActiveGroupIDs != null)
            {
                foreach (var groupId in ctx.ActiveGroupIDs)
                {
                    var groupOwned = genericRepo.GetDXUnits<DXGroupOwnershipUnit>(
                        $"Group = '{groupId}' AND DXUnitDefinition = '{unitDef.ID}'");

                    foreach (var o in groupOwned)
                        result.Add(o.OwnedDXUnitID);
                }
            }

            var publicAccess = genericRepo.GetDXUnits<DXPublicAccessUnit>(
                $"DXUnitDefinition = '{unitDef.ID}'");

            foreach (var access in publicAccess)
            {
                if (access.PublicDXUnitID != Guid.Empty)
                    result.Add(access.PublicDXUnitID);
            }

            return result;
        }

        private static JObject EmptyDXFormat(string? typeName) =>
            JObject.FromObject(new DXDataBlock<DXUnitRecord>
            {
                Meta = new DXMeta { Kind = "DXUnit", Type = typeName },
                Data = new DXData<DXUnitRecord> { Items = [] }
            });

        private static string BuildIdInFilter(IReadOnlyCollection<Guid> ids, string? originalFilter)
        {
            var inList = string.Join(",", ids.Select(x => $"'{x}'"));
            var idIn = $"ID IN ({inList})";
            return string.IsNullOrWhiteSpace(originalFilter)
                ? idIn
                : $"({idIn}) AND ({originalFilter})";
        }

        private bool ShouldApplyAnonymousPublicFallback(DXAccessDecision decision)
        {
            return decision == DXAccessDecision.Denied && executionContextAccessor.Current == null;
        }

        private void ThrowDenied(string typeName, DXUnitTypeAccessOperation operation)
        {
            var subject = GetCurrentSubject();
            logger.LogWarning(
                "Read access denied for subject {Subject} to DX unit type {TypeName} and operation {Operation}.",
                subject,
                typeName,
                operation);
            throw new UnauthorizedAccessException($"Access denied for '{subject}' to '{typeName}' ({operation}).");
        }

        private JObject? MaskSensitive(JObject? jObject, string? typeName)
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

        private void LogReadFailure(string operation, string? typeName, string? error, Guid? id = null, string? filter = null)
        {
            logger.LogError(
                "DX read operation {Operation} failed for type {TypeName}, id {UnitId}, filter {Filter}. Error: {Error}.",
                operation,
                typeName,
                id,
                filter,
                error);
        }

        private string GetCurrentSubject()
        {
            var ctx = executionContextAccessor.Current;
            return ctx == null || string.IsNullOrWhiteSpace(ctx.SubjectId) ? "anonymous" : ctx.SubjectId;
        }
    }
}
