using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.Pipeline;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace IV.DX.Application.Services
{
    internal class DXUnitDataService(
        IDXUnitCoreRepository coreRepo,
        IDXPipelineExecutor dxPipelineExecutor,
        IDXUnitTypeAccessChecker unitTypeAccessChecker,
        IDXUnitGenericRepository genericRepo,
        IDXStructureCache structureCache,
        IDXExecutionContextAccessor executionContextAccessor,
        ILogger<DXUnitDataService> logger) : IDXUnitDataService
    {
        public async Task<Guid> InsertAsync<T>(T dxUnit, DXHandlerBaseContext? context = default, CancellationToken ct = default) where T : DXUnit, new()
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            var typeName = AttributeReader.GetDXUnitTypeName(dxUnit.GetType());
            EnsureWriteAccessForInsert(typeName);

            if (!DXMigrationContext.IsMigrating)
                AssignNewIds(dxUnit);

            var result = await dxPipelineExecutor.InsertAsync(dxUnit, context, ct);

            if (result.IsSuccess)
            {
                TryCreateIdentityOwnership(typeName, dxUnit.Id);
                return dxUnit.Id;
            }
            else
            {
                LogWriteFailure("insert", typeName, result.Error);
                throw new Exception($"There are an error to insert dxUnit: {result.Error}");
            }
        }

        public async Task<Guid> InsertOrUpdateAsync<T>(T dxUnit, DXHandlerBaseContext? context = default, CancellationToken ct = default) where T : DXUnit, new()
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            var typeName = AttributeReader.GetDXUnitTypeName(dxUnit.GetType());
            EnsureWriteAccessForInsert(typeName);

            var itemIsExisting = coreRepo.IsItemExisting(typeName, dxUnit.Id);

            if (itemIsExisting)
            {
                return await UpdateAsync(dxUnit, context, ct);
            }
            else
            {
                return await InsertAsync(dxUnit, context, ct);
            }
        }

        public async Task<Guid> UpdateAsync<T>(T dxUnit, DXHandlerBaseContext? context = default, CancellationToken ct = default) where T : DXUnit, new()
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            var typeName = AttributeReader.GetDXUnitTypeName(dxUnit.GetType());
            EnsureWriteAccessForInstance(typeName, dxUnit.Id);

            var result = await dxPipelineExecutor.UpdateAsync(dxUnit, context, ct);

            if (result.IsSuccess)
            {
                return dxUnit.Id;
            }
            else
            {
                LogWriteFailure("update", typeName, result.Error, dxUnit.Id);
                throw new Exception($"There are an error to update dxUnit: {result.Error}");
            }
        }

        public async Task<bool> IsItemExistingAsync(string typeName, Guid id, DXHandlerBaseContext? context = default, CancellationToken ct = default)
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            EnsureReadAccess(typeName);

            var result = await dxPipelineExecutor.IsUnitExistingAsync(typeName, id, context, ct);

            if (result.IsSuccess)
            {
                if (result.Outcome == DXOutcome.Ok)
                {
                    return result.Value;
                }
            }

            logger.LogError(
                "Existence check failed for DX unit type {TypeName} and id {UnitId}. Error: {Error}.",
                typeName,
                id,
                result.Error);
            throw new Exception($"There are an error to check dxModel existing by type ({typeName}) and id ({id}): {result.Error}");
        }

        public async Task<bool> DeleteAsync<T>(T dxUnit, DXHandlerBaseContext? context = default, CancellationToken ct = default) where T : DXUnit, new()
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            var typeName = AttributeReader.GetDXUnitTypeName(dxUnit.GetType());
            EnsureDeleteAccessForInstance(typeName, dxUnit.Id);

            var result = await dxPipelineExecutor.DeleteAsync(dxUnit, context, ct);

            if (result.IsSuccess)
            {
                TryDeleteOwnership(typeName, dxUnit.Id);
                return true;
            }
            else
            {
                LogDeleteFailure(typeName, result.Error, dxUnit.Id);
                return false;
            }
        }

        public async Task<Guid> InsertAsync(JObject jObject, DXHandlerBaseContext? context = default, CancellationToken ct = default)
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            var typeName = ExtractTypeName(jObject);
            EnsureWriteAccessForInsert(typeName);

            var block = jObject.ToObject<DXDataBlock<DXUnitRecord>>()
                ?? throw new Exception("Invalid DXDataBlock payload.");

            if (!DXMigrationContext.IsMigrating)
                AssignNewIds(block);

            var result = await dxPipelineExecutor.InsertAsync(block, context, ct);

            if (result.IsSuccess)
            {
                TryCreateIdentityOwnership(typeName, result.Value);
                return result.Value;
            }
            else
            {
                LogWriteFailure("insert", typeName, result.Error);
                throw new Exception($"There are an error to insert dxUnit: {result.Error}");
            }
        }

        public async Task<Guid> UpdateAsync(JObject jObject, DXHandlerBaseContext? context = null, CancellationToken ct = default)
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            var typeName = ExtractTypeName(jObject);
            var instanceId = ExtractInstanceId(jObject);
            if (instanceId.HasValue)
                EnsureWriteAccessForInstance(typeName, instanceId.Value);
            else
                EnsureWriteAccessForInsert(typeName);

            var result = await dxPipelineExecutor.UpdateAsync(jObject, context, ct);

            if (result.IsSuccess)
            {
                return result.Value;
            }
            else
            {
                LogWriteFailure("update", typeName, result.Error, instanceId);
                throw new Exception($"There are an error to update dxUnit: {result.Error}");
            }
        }

        public async Task<bool> DeleteAsync(JObject jObject, DXHandlerBaseContext? context = default, CancellationToken ct = default)
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            var typeName = ExtractTypeName(jObject);
            var instanceId = ExtractInstanceId(jObject);
            if (instanceId.HasValue)
                EnsureDeleteAccessForInstance(typeName, instanceId.Value);
            else
                EnsureWriteAccessForInsert(typeName);

            var result = await dxPipelineExecutor.DeleteAsync(jObject, context, ct);

            if (result.IsSuccess)
            {
                if (instanceId.HasValue && typeName != null)
                    TryDeleteOwnership(typeName, instanceId.Value);
                return true;
            }
            else
            {
                LogDeleteFailure(typeName, result.Error, instanceId);
                return false;
            }
        }

        public async Task<Guid> InsertOrUpdateAsync(JObject jObject, DXHandlerBaseContext? context = null, CancellationToken ct = default)
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            EnsureWriteAccessForInsert(ExtractTypeName(jObject));

            var block = jObject.ToObject<DXDataBlock<DXUnitRecord>>()
                ?? throw new Exception("Invalid DXDataBlock payload.");

            var ids = await InsertOrUpdateAsync(block, context, ct);
            return ids.Single();
        }

        public async Task<Guid> InsertAsync(DXDataBlock<DXUnitRecord> block, DXHandlerBaseContext? context = default, CancellationToken ct = default)
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            EnsureWriteAccessForInsert(block?.Meta?.Type);

            if (!DXMigrationContext.IsMigrating)
                AssignNewIds(block);

            var result = await dxPipelineExecutor.InsertAsync(block!, context, ct);

            if (result.IsSuccess)
            {
                TryCreateIdentityOwnership(block?.Meta?.Type, result.Value);
                return result.Value;
            }
            else
            {
                LogWriteFailure("insert", block?.Meta?.Type, result.Error);
                throw new Exception($"There are an error to insert dxUnit: {result.Error}");
            }
        }

        public async Task<Guid> UpdateAsync(DXDataBlock<DXUnitRecord> block, DXHandlerBaseContext? context = null, CancellationToken ct = default)
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            var typeName = block?.Meta?.Type;
            if (!string.IsNullOrWhiteSpace(typeName) && block?.Data?.Items != null)
            {
                foreach (var record in block.Data.Items)
                {
                    if (record != null)
                        EnsureWriteAccessForInstance(typeName, record.Id);
                }
            }
            else
            {
                EnsureWriteAccessForInsert(typeName);
            }

            var result = await dxPipelineExecutor.UpdateAsync(block!, context, ct);

            if (result.IsSuccess)
            {
                return result.Value;
            }
            else
            {
                LogWriteFailure("update", typeName, result.Error);
                throw new Exception($"There are an error to update dxUnit: {result.Error}");
            }
        }

        public async Task<bool> DeleteAsync(DXDataBlock<DXUnitRecord> block, DXHandlerBaseContext? context = default, CancellationToken ct = default)
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            var typeName = block?.Meta?.Type;
            var deleteRefs = block?.Data?.Delete;
            if (!string.IsNullOrWhiteSpace(typeName) && deleteRefs != null)
            {
                foreach (var deleteRef in deleteRefs)
                {
                    if (deleteRef != null)
                        EnsureDeleteAccessForInstance(typeName, deleteRef.Id);
                }
            }
            else
            {
                EnsureWriteAccessForInsert(typeName);
            }

            var result = await dxPipelineExecutor.DeleteAsync(block!, context, ct);

            if (result.IsSuccess)
            {
                if (!string.IsNullOrWhiteSpace(typeName) && deleteRefs != null)
                {
                    foreach (var deleteRef in deleteRefs)
                    {
                        if (deleteRef != null)
                            TryDeleteOwnership(typeName, deleteRef.Id);
                    }
                }
                return true;
            }
            else
            {
                LogDeleteFailure(typeName, result.Error);
                return false;
            }
        }

        public async Task<IEnumerable<Guid>> InsertOrUpdateAsync(DXDataBlock<DXUnitRecord> block, DXHandlerBaseContext? context = null, CancellationToken ct = default)
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            ArgumentNullException.ThrowIfNull(block);
            var typeName = block.Meta?.Type;
            EnsureWriteAccessForInsert(typeName);

            var output = new List<Guid>();

            if (block.Data?.Items != null)
            {
                foreach (var record in block.Data.Items)
                {
                    if (record == null) continue;

                    var itemIsExisting = !string.IsNullOrWhiteSpace(typeName)
                        && await this.IsItemExistingAsync(typeName, record.Id, context, ct);

                    var singleBlock = new DXDataBlock<DXUnitRecord>
                    {
                        Meta = block.Meta!,
                        Data = new DXData<DXUnitRecord>
                        {
                            Items = new List<DXUnitRecord> { record }
                        }
                    };

                    var id = itemIsExisting
                        ? await UpdateAsync(singleBlock, context, ct)
                        : await InsertAsync(singleBlock, context, ct);

                    output.Add(id);
                }
            }

            if (block.Data?.Delete != null && block.Data.Delete.Count > 0)
            {
                var deleteBlock = new DXDataBlock<DXUnitRecord>
                {
                    Meta = block.Meta!,
                    Data = new DXData<DXUnitRecord>
                    {
                        Delete = block.Data.Delete
                    }
                };

                await DeleteAsync(deleteBlock, context, ct);
            }

            return output;
        }

        private static string? ExtractTypeName(JObject jObject)
        {
            return jObject["Meta"]?["Type"]?.ToString();
        }

        private static Guid? ExtractInstanceId(JObject jObject)
        {
            var idToken = jObject["Data"]?["Items"]?[0]?["Id"] ?? jObject["Id"];
            if (idToken != null && Guid.TryParse(idToken.ToString(), out var id))
                return id;
            return null;
        }

        private void EnsureReadAccess(string? typeName)
        {
            if (!string.IsNullOrWhiteSpace(typeName))
            {
                unitTypeAccessChecker.EnsureAccess(typeName, DXUnitTypeAccessOperation.Read);
            }
        }

        private void EnsureDeleteAccessForInstance(string? typeName, Guid instanceId)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return;

            var decision = unitTypeAccessChecker.CheckAccess(typeName, DXUnitTypeAccessOperation.Delete);

            if (decision == DXAccessDecision.Allowed)
                return;

            if (decision == DXAccessDecision.AllowedOwnedOnly)
            {
                EnsureOwnership(typeName, instanceId);
                return;
            }

            var subject = GetCurrentSubject();
            logger.LogWarning(
                "Delete access denied for subject {Subject} to DX unit type {TypeName} and instance {InstanceId}.",
                subject,
                typeName,
                instanceId);
            throw new UnauthorizedAccessException($"Access denied for '{subject}' to '{typeName}' ({DXUnitTypeAccessOperation.Delete}).");
        }

        private void EnsureWriteAccessForInsert(string? typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return;

            var decision = unitTypeAccessChecker.CheckAccess(typeName, DXUnitTypeAccessOperation.Write);

            if (decision == DXAccessDecision.Allowed)
                return;

            // AllowedOwnedOnly or Denied: cannot create without type-level write grant
            var subject = GetCurrentSubject();
            logger.LogWarning(
                "Write access denied for subject {Subject} to create DX unit type {TypeName}.",
                subject,
                typeName);
            throw new UnauthorizedAccessException($"Access denied for '{subject}' to '{typeName}' ({DXUnitTypeAccessOperation.Write}).");
        }

        private void EnsureWriteAccessForInstance(string? typeName, Guid instanceId)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return;

            var decision = unitTypeAccessChecker.CheckAccess(typeName, DXUnitTypeAccessOperation.Write);

            if (decision == DXAccessDecision.Allowed)
                return;

            if (decision == DXAccessDecision.AllowedOwnedOnly)
            {
                EnsureOwnership(typeName, instanceId);
                return;
            }

            var subject = GetCurrentSubject();
            logger.LogWarning(
                "Write access denied for subject {Subject} to DX unit type {TypeName} and instance {InstanceId}.",
                subject,
                typeName,
                instanceId);
            throw new UnauthorizedAccessException($"Access denied for '{subject}' to '{typeName}' ({DXUnitTypeAccessOperation.Write}).");
        }

        private void EnsureOwnership(string typeName, Guid instanceId)
        {
            var unitDef = structureCache.GetDXUnit(typeName);
            if (unitDef == null || !unitDef.SupportsOwnership)
            {
                var subject = GetCurrentSubject();
                logger.LogWarning(
                    "Ownership check denied for subject {Subject} because DX unit type {TypeName} does not support ownership.",
                    subject,
                    typeName);
                throw new UnauthorizedAccessException($"Access denied for '{subject}' to '{typeName}' instance '{instanceId}'.");
            }

            var context = executionContextAccessor.Current;

            if (context?.IdentityId.HasValue == true)
            {
                var identityOwnership = genericRepo
                    .GetDXUnits<DXIdentityOwnershipUnit>(
                        $"Identity = '{context.IdentityId.Value}' AND DXUnitDefinition = '{unitDef.Id}' AND OwnedDXUnitId = '{instanceId}'")
                    .FirstOrDefault();

                if (identityOwnership != null)
                    return;
            }

            if (context?.ActiveGroupIDs != null)
            {
                foreach (var groupId in context.ActiveGroupIDs)
                {
                    var groupOwnership = genericRepo
                        .GetDXUnits<DXGroupOwnershipUnit>(
                            $"Group = '{groupId}' AND DXUnitDefinition = '{unitDef.Id}' AND OwnedDXUnitId = '{instanceId}'")
                        .FirstOrDefault();

                    if (groupOwnership != null)
                        return;
                }
            }

            var subjectId = GetCurrentSubject();
            logger.LogWarning(
                "Ownership check denied for subject {Subject} to DX unit type {TypeName} and instance {InstanceId}.",
                subjectId,
                typeName,
                instanceId);
            throw new UnauthorizedAccessException($"Access denied for '{subjectId}' to '{typeName}' instance '{instanceId}'.");
        }

        private void TryCreateIdentityOwnership(string? typeName, Guid unitId)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return;

            var ctx = executionContextAccessor.Current;
            if (ctx == null || ctx.IsSystem || !ctx.IdentityId.HasValue)
                return;

            var unitDef = structureCache.GetDXUnit(typeName);
            if (unitDef == null || unitDef.Kind == DXObjectKindEnum.Core || !unitDef.SupportsOwnership)
                return;

            var ownership = new DXIdentityOwnershipUnit
            {
                Id = Guid.CreateVersion7(),
                TimeStamp = DateTime.UtcNow,
                Identity = ctx.IdentityId.Value,
                DXUnitDefinition = unitDef.Id,
                OwnedDXUnitId = unitId
            };

            genericRepo.Insert(ownership);
        }


        private static void AssignNewIds(DXUnit dxUnit)
        {
            dxUnit.Id = Guid.CreateVersion7();

            foreach (var prop in AttributeReader.GetSingleItemInfos(dxUnit))
            {
                var element = (DXElement)prop.GetValue(dxUnit)!;
                element.Id = Guid.CreateVersion7();
                element.DXUnitId = dxUnit.Id;
            }

            foreach (var prop in AttributeReader.GetMultiItemInfos(dxUnit))
            {
                var container = prop.GetValue(dxUnit)!;
                var announced = (System.Collections.IEnumerable)container.GetType().GetProperty("Announced")!.GetValue(container)!;
                foreach (DXElement element in announced)
                {
                    element.Id = Guid.CreateVersion7();
                    element.DXUnitId = dxUnit.Id;
                }
            }
        }

        private static void AssignNewIds(DXDataBlock<DXUnitRecord>? block)
        {
            if (block?.Data?.Items == null) return;

            var idMap = new Dictionary<Guid, Guid>();

            foreach (var record in block.Data.Items)
            {
                var oldId = record.Id;
                var newId = Guid.CreateVersion7();
                if (oldId != Guid.Empty)
                    idMap[oldId] = newId;
                record.Id = newId;
            }

            foreach (var record in block.Data.Items)
            {
                if (record.DXElements == null) continue;
                foreach (var elementBlock in record.DXElements.Values)
                {
                    if (elementBlock?.Data?.Items == null) continue;
                    foreach (var elementRecord in elementBlock.Data.Items)
                    {
                        elementRecord.Id = Guid.CreateVersion7();
                        elementRecord.DXUnitId = record.Id;

                        if (elementRecord.Fields == null) continue;
                        foreach (var key in elementRecord.Fields.Keys.ToList())
                        {
                            var token = elementRecord.Fields[key];
                            if (token == null || token.Type == JTokenType.Null) continue;
                            if (token.Type == JTokenType.String &&
                                Guid.TryParse(token.Value<string>(), out var fieldGuid) &&
                                idMap.TryGetValue(fieldGuid, out var remappedGuid))
                            {
                                elementRecord.Fields[key] = JToken.FromObject(remappedGuid);
                            }
                        }
                    }
                }
            }
        }

        private void TryDeleteOwnership(string typeName, Guid unitId)
        {
            var unitDef = structureCache.GetDXUnit(typeName);
            if (unitDef == null || unitDef.Kind == DXObjectKindEnum.Core || !unitDef.SupportsOwnership)
                return;

            var identityOwners = genericRepo
                .GetDXUnits<DXIdentityOwnershipUnit>(
                    $"DXUnitDefinition = '{unitDef.Id}' AND OwnedDXUnitId = '{unitId}'")
                .ToList();

            foreach (var owner in identityOwners)
                genericRepo.Delete(owner);

            var groupOwners = genericRepo
                .GetDXUnits<DXGroupOwnershipUnit>(
                    $"DXUnitDefinition = '{unitDef.Id}' AND OwnedDXUnitId = '{unitId}'")
                .ToList();

            foreach (var owner in groupOwners)
                genericRepo.Delete(owner);
        }

        private string GetCurrentSubject()
        {
            var ctx = executionContextAccessor.Current;
            return ctx == null || string.IsNullOrWhiteSpace(ctx.SubjectId) ? "anonymous" : ctx.SubjectId;
        }

        private void LogWriteFailure(string operation, string? typeName, string? error, Guid? id = null)
        {
            logger.LogError(
                "DX {Operation} failed for type {TypeName}, id {UnitId}. Error: {Error}.",
                operation,
                typeName,
                id,
                error);
        }

        private void LogDeleteFailure(string? typeName, string? error, Guid? id = null)
        {
            logger.LogWarning(
                "DX delete failed for type {TypeName}, id {UnitId}. Error: {Error}.",
                typeName,
                id,
                error);
        }
    }
}
