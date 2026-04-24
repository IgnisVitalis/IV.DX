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
    internal class DXUnitDataService(
        IDXUnitCoreRepository coreRepo,
        IDXPipelineExecutor dxPipelineExecutor,
        IDXUnitTypeAccessChecker unitTypeAccessChecker,
        IDXUnitGenericRepository genericRepo,
        IDXStructureCache structureCache,
        IDXExecutionContextAccessor executionContextAccessor,
        ILogger<DXUnitDataService> logger) : IDXUnitDataService
    {
        public async Task<T> InsertAsync<T>(T dxUnit, DXHandlerBaseContext? context = default, CancellationToken ct = default) where T : DXUnit, new()
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            var typeName = AttributeReader.GetDXUnitTypeName(dxUnit.GetType());
            EnsureWriteAccessForInsert(typeName);

            var result = await dxPipelineExecutor.InsertAsync(dxUnit, context, ct);

            if (result.IsSuccess && result.Value != null)
            {
                TryCreateIdentityOwnership(typeName, result.Value.ID);
                return result.Value;
            }
            else
            {
                LogWriteFailure("insert", typeName, result.Error);
                throw new Exception($"There are an error to insert dxUnit: {result.Error}");
            }
        }

        public async Task<T> InsertOrUpdateAsync<T>(T dxUnit, DXHandlerBaseContext? context = default, CancellationToken ct = default) where T : DXUnit, new()
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            var typeName = AttributeReader.GetDXUnitTypeName(dxUnit.GetType());
            EnsureWriteAccessForInsert(typeName);

            var itemIsExisting = coreRepo.IsItemExisting(typeName, dxUnit.ID);

            if (itemIsExisting)
            {
                return await UpdateAsync(dxUnit, context, ct);
            }
            else
            {
                return await InsertAsync(dxUnit, context, ct);
            }
        }

        public async Task<T> UpdateAsync<T>(T dxUnit, DXHandlerBaseContext? context = default, CancellationToken ct = default) where T : DXUnit, new()
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            var typeName = AttributeReader.GetDXUnitTypeName(dxUnit.GetType());
            EnsureWriteAccessForInstance(typeName, dxUnit.ID);

            var result = await dxPipelineExecutor.UpdateAsync(dxUnit, context, ct);

            if (result.IsSuccess && result.Value != null)
            {
                return result.Value;
            }
            else
            {
                LogWriteFailure("update", typeName, result.Error, dxUnit.ID);
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
            EnsureDeleteAccessForInstance(typeName, dxUnit.ID);

            var result = await dxPipelineExecutor.DeleteAsync(dxUnit, context, ct);

            if (result.IsSuccess)
            {
                TryDeleteOwnership(typeName, dxUnit.ID);
                return true;
            }
            else
            {
                LogDeleteFailure(typeName, result.Error, dxUnit.ID);
                return false;
            }
        }

        public async Task<JObject> InsertAsync(JObject jObject, DXHandlerBaseContext? context = default, CancellationToken ct = default)
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            var typeName = ExtractTypeName(jObject);
            EnsureWriteAccessForInsert(typeName);

            var result = await dxPipelineExecutor.InsertAsync(jObject, context, ct);

            if (result.IsSuccess && result.Value != null)
            {
                TryCreateIdentityOwnershipFromJObject(typeName, result.Value);
                return result.Value;
            }
            else
            {
                LogWriteFailure("insert", typeName, result.Error);
                throw new Exception($"There are an error to insert dxUnit: {result.Error}");
            }
        }

        public async Task<JObject> UpdateAsync(JObject jObject, DXHandlerBaseContext? context = null, CancellationToken ct = default)
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

            if (result.IsSuccess && result.Value != null)
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

        public async Task<JObject> InsertOrUpdateAsync(JObject jObject, DXHandlerBaseContext? context = null, CancellationToken ct = default)
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            EnsureWriteAccessForInsert(ExtractTypeName(jObject));

            var block = jObject.ToObject<DXDataBlock<DXUnitRecord>>();
            if (block == null)
            {
                logger.LogError("InsertOrUpdate failed because the DXDataBlock payload is invalid.");
                throw new Exception("Invalid DXDataBlock payload.");
            }

            var processed = await InsertOrUpdateAsync(block, context, ct);
            return JObject.FromObject(processed);
        }

        public async Task<DXDataBlock<DXUnitRecord>> InsertAsync(DXDataBlock<DXUnitRecord> block, DXHandlerBaseContext? context = default, CancellationToken ct = default)
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            EnsureWriteAccessForInsert(block?.Meta?.Type);

            var result = await dxPipelineExecutor.InsertAsync(block!, context, ct);

            if (result.IsSuccess && result.Value != null)
            {
                TryCreateIdentityOwnershipFromBlock(result.Value);
                return result.Value;
            }
            else
            {
                LogWriteFailure("insert", block?.Meta?.Type, result.Error);
                throw new Exception($"There are an error to insert dxUnit: {result.Error}");
            }
        }

        public async Task<DXDataBlock<DXUnitRecord>> UpdateAsync(DXDataBlock<DXUnitRecord> block, DXHandlerBaseContext? context = null, CancellationToken ct = default)
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
                        EnsureWriteAccessForInstance(typeName, record.ID);
                }
            }
            else
            {
                EnsureWriteAccessForInsert(typeName);
            }

            var result = await dxPipelineExecutor.UpdateAsync(block!, context, ct);

            if (result.IsSuccess && result.Value != null)
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
                        EnsureDeleteAccessForInstance(typeName, deleteRef.ID);
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
                            TryDeleteOwnership(typeName, deleteRef.ID);
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

        public async Task<DXDataBlock<DXUnitRecord>> InsertOrUpdateAsync(DXDataBlock<DXUnitRecord> block, DXHandlerBaseContext? context = null, CancellationToken ct = default)
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            ArgumentNullException.ThrowIfNull(block);
            var typeName = block.Meta?.Type;
            EnsureWriteAccessForInsert(typeName);

            var output = new List<DXUnitRecord>();

            if (block.Data?.Items != null)
            {
                foreach (var record in block.Data.Items)
                {
                    if (record == null) continue;

                    var itemIsExisting = !string.IsNullOrWhiteSpace(typeName)
                        && await this.IsItemExistingAsync(typeName, record.ID, context, ct);

                    var singleBlock = new DXDataBlock<DXUnitRecord>
                    {
                        Meta = block.Meta!,
                        Data = new DXData<DXUnitRecord>
                        {
                            Items = new List<DXUnitRecord> { record }
                        }
                    };

                    var processed = itemIsExisting
                        ? await UpdateAsync(singleBlock, context, ct)
                        : await InsertAsync(singleBlock, context, ct);

                    if (processed.Data?.Items != null)
                        output.AddRange(processed.Data.Items);
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

            return new DXDataBlock<DXUnitRecord>
            {
                Meta = block.Meta!,
                Data = new DXData<DXUnitRecord>
                {
                    Items = output.Count == 0 ? null : output,
                    Delete = block.Data?.Delete
                }
            };
        }

        private static string? ExtractTypeName(JObject jObject)
        {
            return jObject["Meta"]?["Type"]?.ToString();
        }

        private static Guid? ExtractInstanceId(JObject jObject)
        {
            var idToken = jObject["Data"]?["Items"]?[0]?["ID"] ?? jObject["ID"];
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

            if (context?.IdentityID.HasValue == true)
            {
                var identityOwnership = genericRepo
                    .GetDXUnits<DXIdentityOwnershipUnit>(
                        $"Identity = '{context.IdentityID.Value}' AND DXUnitDefinition = '{unitDef.ID}' AND OwnedDXUnitID = '{instanceId}'")
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
                            $"Group = '{groupId}' AND DXUnitDefinition = '{unitDef.ID}' AND OwnedDXUnitID = '{instanceId}'")
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
            if (ctx == null || ctx.IsSystem || !ctx.IdentityID.HasValue)
                return;

            var unitDef = structureCache.GetDXUnit(typeName);
            if (unitDef == null || unitDef.Kind == DXObjectKindEnum.Core || !unitDef.SupportsOwnership)
                return;

            var ownership = new DXIdentityOwnershipUnit
            {
                ID = Guid.NewGuid(),
                TimeStamp = DateTime.UtcNow,
                Identity = ctx.IdentityID.Value,
                DXUnitDefinition = unitDef.ID,
                OwnedDXUnitID = unitId
            };

            genericRepo.Insert(ownership);
        }

        private void TryCreateIdentityOwnershipFromJObject(string? typeName, JObject resultValue)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return;

            var block = resultValue.ToObject<DXDataBlock<DXUnitRecord>>();
            if (block?.Data?.Items == null)
                return;

            foreach (var item in block.Data.Items)
            {
                if (item != null && item.ID != Guid.Empty)
                    TryCreateIdentityOwnership(typeName, item.ID);
            }
        }

        private void TryCreateIdentityOwnershipFromBlock(DXDataBlock<DXUnitRecord> block)
        {
            var typeName = block.Meta?.Type;
            if (string.IsNullOrWhiteSpace(typeName) || block.Data?.Items == null)
                return;

            foreach (var item in block.Data.Items)
            {
                if (item != null && item.ID != Guid.Empty)
                    TryCreateIdentityOwnership(typeName, item.ID);
            }
        }

        private void TryDeleteOwnership(string typeName, Guid unitId)
        {
            var unitDef = structureCache.GetDXUnit(typeName);
            if (unitDef == null || unitDef.Kind == DXObjectKindEnum.Core || !unitDef.SupportsOwnership)
                return;

            var identityOwners = genericRepo
                .GetDXUnits<DXIdentityOwnershipUnit>(
                    $"DXUnitDefinition = '{unitDef.ID}' AND OwnedDXUnitID = '{unitId}'")
                .ToList();

            foreach (var owner in identityOwners)
                genericRepo.Delete(owner);

            var groupOwners = genericRepo
                .GetDXUnits<DXGroupOwnershipUnit>(
                    $"DXUnitDefinition = '{unitDef.ID}' AND OwnedDXUnitID = '{unitId}'")
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
