using IV.DX.Application.Contracts.Pipeline;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Converters.DXObjectConverters;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Helpers.DXObjectHelpers;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Reflection;

namespace IV.DX.Application.Pipeline
{
    internal class DXPipelineExecutor(
        IDXStructureCache dxStructureCache,
        IDXUnitCoreRepository coreRepo,
        IDXUnitGenericRepository genericRepo,
        IDXUnitGetHandlerProvider getHandlerProvider,
        IDXUnitInsertHandlerProvider insertHandlerProvider,
        IDXUnitUpdateHandlerProvider updateHandlerProvider,
        IDXUnitDeleteHandlerProvider deleteHandlerProvider)
        : IDXPipelineExecutor
    {
        public async Task<DXResult<T?>> GetAsync<T>(
            Guid id, DXHandlerBaseContext ctx, CancellationToken ct) where T : DXUnit, new()
        {
            var flow = DXFlow.Continue;
            T? dxUnit = default;

            foreach (var h in getHandlerProvider.GetBeforeGetHandlers<T>())
            {
                var r = await h.BeforeGetAsync(id, ctx, ct);
                if (!r.IsSuccess) return DXResult<T?>.Fail(r.Error!);

                if (r.Flow == DXFlow.SkipProcess) flow = DXFlow.SkipProcess;
                if (r.Flow == DXFlow.Stop) return DXResult<T?>.OkStop();
            }

            if (flow != DXFlow.SkipProcess)
            {
                dxUnit = genericRepo.GetDXUnit<T>(id);
                if (dxUnit is null) return DXResult<T?>.NotFound();
            }

            foreach (var h in getHandlerProvider.GetAfterGetHandlers<T>())
            {
                var r = await h.AfterGetAsync(dxUnit, ctx, ct);
                if (!r.IsSuccess) return DXResult<T?>.Fail(r.Error!);
            }

            return flow switch
            {
                DXFlow.SkipProcess => DXResult<T?>.OkSkipProcess(dxUnit),
                DXFlow.Stop => DXResult<T?>.OkStop(dxUnit),
                _ => DXResult<T?>.OkContinue(dxUnit),
            };
        }

        public async Task<DXResult<JObject?>> GetAsync(
            string typeName, Guid id, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            if (getHandlerProvider.TryResolveType(typeName, out var modelType))
            {
                var inv = GetGetInvoker(modelType);
                var baseRes = await inv(this, id, ctx, ct);

                if (!baseRes.IsSuccess)
                    return DXResult<JObject?>.Fail(baseRes.Error!);

                var block = baseRes.Value is null
                    ? null
                    : JObject.FromObject(DXRecordWriter.ToBlock(baseRes.Value));

                return DXResult<JObject?>.Ok(block, baseRes.Flow);
            }

            var recordBlock = coreRepo.GetItemRecord(typeName, id);

            if (recordBlock is null) return DXResult<JObject?>.NotFound();

            return DXResult<JObject?>.OkContinue(JObject.FromObject(recordBlock));
        }

        public async Task<DXResult<T>> InsertAsync<T>(
            T dxUnit,
            DXHandlerBaseContext ctx,
            CancellationToken ct) where T : DXUnit, new()
        {
            var dxUnitToProcess = dxUnit;
            ctx.OriginalItem = dxUnit;

            var flow = DXFlow.Continue;

            foreach (var h in insertHandlerProvider.GetBeforeInsertHandlers<T>())
            {
                var r = await h.BeforeInsertAsync(dxUnitToProcess, ctx, ct);
                if (!r.IsSuccess) return DXResult<T>.Fail(r.Error!);

                dxUnitToProcess = r.Value!;
                if (r.Flow == DXFlow.SkipProcess) flow = DXFlow.SkipProcess;
                if (r.Flow == DXFlow.Stop) return DXResult<T>.OkStop(dxUnitToProcess);
            }

            if (flow != DXFlow.SkipProcess)
            {
                var id = genericRepo.Insert(dxUnitToProcess);
                var reloaded = genericRepo.GetDXUnit<T>(id);

                if (reloaded is null) return DXResult<T>.Fail("Inserted dxUnit not found.");
                dxUnitToProcess = reloaded;
            }

            foreach (var h in insertHandlerProvider.GetAfterInsertHandlers<T>())
            {
                var r = await h.AfterInsertAsync(dxUnitToProcess, ctx, ct);
                if (!r.IsSuccess) return DXResult<T>.Fail(r.Error!);
            }

            return flow switch
            {
                DXFlow.SkipProcess => DXResult<T>.OkSkipProcess(dxUnitToProcess),
                DXFlow.Stop => DXResult<T>.OkStop(dxUnitToProcess),
                _ => DXResult<T>.OkContinue(dxUnitToProcess),
            };
        }

        public async Task<DXResult<JObject>> InsertAsync(
            JObject jObject,
            DXHandlerBaseContext ctx,
            CancellationToken ct)
        {
            var block = jObject.ToObject<DXDataBlock<DXUnitRecord>>();
            if (block == null)
                return DXResult<JObject>.Fail("Invalid DXDataBlock payload.");

            var baseRes = await InsertAsync(block, ctx, ct);
            if (!baseRes.IsSuccess) return DXResult<JObject>.Fail(baseRes.Error!);

            return DXResult<JObject>.Ok(JObject.FromObject(baseRes.Value!), baseRes.Flow);
        }

        public async Task<DXResult<T>> UpdateAsync<T>(
            T dxUnit,
            DXHandlerBaseContext ctx,
            CancellationToken ct) where T : DXUnit, new()
        {
            var dxUnitToProcess = dxUnit;
            ctx.OriginalItem = dxUnit;

            var flow = DXFlow.Continue;

            foreach (var h in updateHandlerProvider.GetBeforeUpdateHandlers<T>())
            {
                var r = await h.BeforeUpdateAsync(dxUnitToProcess, ctx, ct);
                if (!r.IsSuccess) return DXResult<T>.Fail(r.Error!);

                dxUnitToProcess = r.Value!;
                if (r.Flow == DXFlow.SkipProcess) flow = DXFlow.SkipProcess;
                if (r.Flow == DXFlow.Stop) return DXResult<T>.OkStop(dxUnitToProcess);
            }

            if (flow != DXFlow.SkipProcess)
            {
                var id = genericRepo.Update(dxUnitToProcess);
                var reloaded = genericRepo.GetDXUnit<T>(id);

                if (reloaded is null) return DXResult<T>.Fail("Inserted dxUnit not found.");
                dxUnitToProcess = reloaded;
            }

            foreach (var h in updateHandlerProvider.GetAfterUpdateHandlers<T>())
            {
                var r = await h.AfterUpdateAsync(dxUnitToProcess, ctx, ct);
                if (!r.IsSuccess) return DXResult<T>.Fail(r.Error!);
            }

            return flow switch
            {
                DXFlow.SkipProcess => DXResult<T>.OkSkipProcess(dxUnitToProcess),
                DXFlow.Stop => DXResult<T>.OkStop(dxUnitToProcess),
                _ => DXResult<T>.OkContinue(dxUnitToProcess),
            };
        }

        public async Task<DXResult<JObject>> UpdateAsync(
            JObject jObject,
            DXHandlerBaseContext ctx,
            CancellationToken ct)
        {
            var block = jObject.ToObject<DXDataBlock<DXUnitRecord>>();
            if (block == null)
                return DXResult<JObject>.Fail("Invalid DXDataBlock payload.");

            var baseRes = await UpdateAsync(block, ctx, ct);
            if (!baseRes.IsSuccess) return DXResult<JObject>.Fail(baseRes.Error!);

            return DXResult<JObject>.Ok(JObject.FromObject(baseRes.Value!), baseRes.Flow);
        }

        public async Task<DXResult<T>> DeleteAsync<T>(T dxUnit, DXHandlerBaseContext ctx, CancellationToken ct) where T : DXUnit, new()
        {
            var existingDXUnit = await this.GetAsync<T>(dxUnit.ID, ctx, ct);

            if (!existingDXUnit.IsSuccess || !existingDXUnit.HasValue)
                return DXResult<T>.NotFound();

            var dxUnitToProcess = existingDXUnit.Value;
            ctx.OriginalItem = dxUnit;

            var flow = DXFlow.Continue;

            foreach (var h in deleteHandlerProvider.GetBeforeDeleteHandlers<T>())
            {
                var r = await h.BeforeDeleteAsync(dxUnitToProcess, ctx, ct);
                if (!r.IsSuccess) return DXResult<T>.Fail(r.Error!);

                dxUnitToProcess = r.Value!;
                if (r.Flow == DXFlow.SkipProcess) flow = DXFlow.SkipProcess;
                if (r.Flow == DXFlow.Stop) return DXResult<T>.OkStop(dxUnitToProcess);
            }

            if (flow != DXFlow.SkipProcess)
            {
                var result = genericRepo.Delete(dxUnitToProcess);

                if (!result) return DXResult<T>.Fail("Inserted dxUnit not found.");
            }

            foreach (var h in deleteHandlerProvider.GetAfterDeleteHandlers<T>())
            {
                var r = await h.AfterDeleteAsync(dxUnitToProcess, ctx, ct);
                if (!r.IsSuccess) return DXResult<T>.Fail(r.Error!);
            }

            return flow switch
            {
                DXFlow.SkipProcess => DXResult<T>.OkSkipProcess(dxUnitToProcess),
                DXFlow.Stop => DXResult<T>.OkStop(dxUnitToProcess),
                _ => DXResult<T>.OkContinue(dxUnitToProcess),
            };
        }

        public async Task<DXResult<JObject>> DeleteAsync(JObject jObject, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            var block = jObject.ToObject<DXDataBlock<DXUnitRecord>>();
            if (block == null)
                return DXResult<JObject>.Fail("Invalid DXDataBlock payload.");

            var baseRes = await DeleteAsync(block, ctx, ct);
            if (!baseRes.IsSuccess) return DXResult<JObject>.Fail(baseRes.Error!);

            return DXResult<JObject>.Ok(JObject.FromObject(baseRes.Value!), baseRes.Flow);
        }

        public async Task<DXResult<DXDataBlock<DXUnitRecord>>> InsertAsync(
            DXDataBlock<DXUnitRecord> block,
            DXHandlerBaseContext ctx,
            CancellationToken ct)
        {
            return await ProcessRecordBlockAsync(block, ctx, ct, isUpdate: false);
        }

        public async Task<DXResult<DXDataBlock<DXUnitRecord>>> UpdateAsync(
            DXDataBlock<DXUnitRecord> block,
            DXHandlerBaseContext ctx,
            CancellationToken ct)
        {
            return await ProcessRecordBlockAsync(block, ctx, ct, isUpdate: true);
        }

        public async Task<DXResult<DXDataBlock<DXUnitRecord>>> DeleteAsync(
            DXDataBlock<DXUnitRecord> block,
            DXHandlerBaseContext ctx,
            CancellationToken ct)
        {
            if (block == null)
                return DXResult<DXDataBlock<DXUnitRecord>>.Fail("DXUnitRecord block is null.");

            var typeName = block.Meta?.Type;
            if (string.IsNullOrWhiteSpace(typeName))
                return DXResult<DXDataBlock<DXUnitRecord>>.Fail("Type name not found in block Meta.");

            if (block.Data?.Delete == null || block.Data.Delete.Count == 0)
                return DXResult<DXDataBlock<DXUnitRecord>>.OkContinue(block);

            foreach (var deleteRef in block.Data.Delete)
            {
                if (insertHandlerProvider.TryResolveType(typeName, out var modelType))
                {
                    var dxUnit = (DXUnit)Activator.CreateInstance(modelType)!;
                    dxUnit.ID = deleteRef.ID;

                    var inv = GetDeleteInvoker(modelType);
                    var baseRes = await inv(this, dxUnit, ctx, ct);
                    if (!baseRes.IsSuccess) return DXResult<DXDataBlock<DXUnitRecord>>.Fail(baseRes.Error!);
                }
                else
                {
                    var result = coreRepo.Delete(typeName, deleteRef.ID);
                    if (!result)
                        return DXResult<DXDataBlock<DXUnitRecord>>.Fail("DXUnit delete failed.");
                }
            }

            return DXResult<DXDataBlock<DXUnitRecord>>.OkContinue(block);
        }

        private async Task<DXResult<DXDataBlock<DXUnitRecord>>> ProcessRecordBlockAsync(
            DXDataBlock<DXUnitRecord> block,
            DXHandlerBaseContext ctx,
            CancellationToken ct,
            bool isUpdate)
        {
            if (block == null)
                return DXResult<DXDataBlock<DXUnitRecord>>.Fail("DXUnitRecord block is null.");

            var typeName = block.Meta?.Type;
            if (string.IsNullOrWhiteSpace(typeName))
                return DXResult<DXDataBlock<DXUnitRecord>>.Fail("Type name not found in block Meta.");

            var resultBlock = new DXDataBlock<DXUnitRecord>
            {
                Meta = block.Meta,
                Data = new DXData<DXUnitRecord>()
            };

            var records = block.Data?.Items;
            if (records == null || records.Count == 0)
                return DXResult<DXDataBlock<DXUnitRecord>>.OkContinue(resultBlock);

            var output = new List<DXUnitRecord>();

            foreach (var record in records)
            {
                if (record == null) continue;

                if (insertHandlerProvider.TryResolveType(typeName, out var modelType))
                {
                    DXUnit dxUnit;
                    try
                    {
                        dxUnit = DXRecordConverter.ToDXUnit(record, modelType);
                    }
                    catch (Exception e)
                    {
                        return DXResult<DXDataBlock<DXUnitRecord>>.Fail($"Failed to deserialize DXUnit: {e.Message}");
                    }

                    var inv = isUpdate ? GetUpdateInvoker(modelType) : GetInsertInvoker(modelType);
                    var baseRes = await inv(this, dxUnit, ctx, ct);
                    if (!baseRes.IsSuccess) return DXResult<DXDataBlock<DXUnitRecord>>.Fail(baseRes.Error!);

                    var outRecord = DXRecordWriter.ToRecord(baseRes.Value!);
                    output.Add(outRecord);
                }
                else
                {
                    var single = new DXDataBlock<DXUnitRecord>
                    {
                        Meta = block.Meta,
                        Data = new DXData<DXUnitRecord>
                        {
                            Items = new List<DXUnitRecord> { record }
                        }
                    };

                    var id = coreRepo.InsertOrUpdate(single);

                    if (id == Guid.Empty)
                        return DXResult<DXDataBlock<DXUnitRecord>>.Fail("DXUnit insert/update failed.");

                    output.Add(record);
                }
            }

            resultBlock.Data.Items = output.Count == 0 ? null : output;
            return DXResult<DXDataBlock<DXUnitRecord>>.OkContinue(resultBlock);
        }

        public async Task<DXResult<IEnumerable<T>?>> GetItemsAsync<T>(
            IEnumerable<Guid> ids,
            DXHandlerBaseContext ctx,
            CancellationToken ct) where T : DXUnit, new()
        {
            var flow = DXFlow.Continue;
            IEnumerable<T>? dxUnits = default;

            foreach (var h in getHandlerProvider.GetBeforeGetHandlers<T>())
            {
                foreach (var id in ids)
                {
                    var r = await h.BeforeGetAsync(id, ctx, ct);
                    if (!r.IsSuccess) return DXResult<IEnumerable<T>?>.Fail(r.Error!);

                    if (r.Flow == DXFlow.SkipProcess) flow = DXFlow.SkipProcess;
                    if (r.Flow == DXFlow.Stop) return DXResult<IEnumerable<T>?>.OkStop();
                }
            }

            if (flow != DXFlow.SkipProcess)
            {
                dxUnits = genericRepo.GetDXUnits<T>(ids);

                if (dxUnits is null || dxUnits.Count() == 0)
                    return DXResult<IEnumerable<T>?>.NotFound();
            }

            foreach (var h in getHandlerProvider.GetAfterGetHandlers<T>())
            {
                foreach (var dxUnit in dxUnits)
                {
                    var r = await h.AfterGetAsync(dxUnit, ctx, ct);
                    if (!r.IsSuccess) return DXResult<IEnumerable<T>?>.Fail(r.Error!);
                }
            }

            return flow switch
            {
                DXFlow.SkipProcess => DXResult<IEnumerable<T>?>.OkSkipProcess(dxUnits),
                DXFlow.Stop => DXResult<IEnumerable<T>?>.OkStop(dxUnits),
                _ => DXResult<IEnumerable<T>?>.OkContinue(dxUnits),
            };
        }

        public async Task<DXResult<IEnumerable<T>?>> GetItemsAsync<T>(
            string query,
            DXHandlerBaseContext ctx,
            CancellationToken ct) where T : DXUnit, new()
        {
            var typeName = AttributeReader.GetDXUnitTypeName(typeof(T));
            var ids = coreRepo.GetItemIDs(typeName, query);

            return await GetItemsAsync<T>(ids, ctx, ct);
        }

        public async Task<DXResult<IEnumerable<T>?>> GetItemsAsync<T>(
            DXHandlerBaseContext ctx,
            CancellationToken ct) where T : DXUnit, new()
        {
            return await GetItemsAsync<T>(string.Empty, ctx, ct);
        }

        public async Task<DXResult<IEnumerable<JObject>?>> GetItemsAsync(
            string typeName,
            IEnumerable<Guid> ids,
            DXHandlerBaseContext ctx,
            CancellationToken ct)
        {
            if (getHandlerProvider.TryResolveType(typeName, out var modelType))
            {
                var inv = GetGetItemsInvoker(modelType);

                var baseRes = await inv(this, ids, ctx, ct);

                if (!baseRes.IsSuccess)
                    return DXResult<IEnumerable<JObject>?>.Fail(baseRes.Error!);

                if (baseRes.Value == null)
                    return DXResult<IEnumerable<JObject>?>.Ok(null, baseRes.Flow);

                var upsertRecords = baseRes.Value.Select(x => DXRecordWriter.ToRecord(x)).ToList();
                var block = new DXDataBlock<DXUnitRecord>
                {
                    Meta = new DXMeta { Kind = "DXUnit", Type = typeName },
                    Data = new DXData<DXUnitRecord> { Items = upsertRecords }
                };

                return DXResult<IEnumerable<JObject>?>.Ok(new List<JObject> { JObject.FromObject(block) }, baseRes.Flow);
            }

            var blockRaw = coreRepo.GetItemsRecord(typeName, ids);
            var records = blockRaw.Data?.Items;

            if (records == null || records.Count == 0)
                return DXResult<IEnumerable<JObject>?>.NotFound();

            return DXResult<IEnumerable<JObject>?>.OkContinue(new List<JObject> { JObject.FromObject(blockRaw) });
        }

        public async Task<DXResult<IEnumerable<JObject>?>> GetItemsAsync(
            string typeName,
            string query,
            DXHandlerBaseContext ctx,
            CancellationToken ct)
        {
            var ids = coreRepo.GetItemIDs(typeName, query);

            return await GetItemsAsync(typeName, ids, ctx, ct);
        }

        public async Task<DXResult<IEnumerable<JObject>?>> GetItemsAsync(
            string typeName,
            DXHandlerBaseContext ctx,
            CancellationToken ct)
        {
            return await GetItemsAsync(typeName, string.Empty, ctx, ct);
        }

        public async Task<DXResult<bool>> IsUnitExistingAsync<T>(Guid id, DXHandlerBaseContext ctx, CancellationToken ct) where T : DXUnit, new()
        {
            var flow = DXFlow.Continue;
            bool result = false;

            foreach (var h in getHandlerProvider.GetIsItemExistingHandlers<T>())
            {
                var r = await h.IsItemExistingAsync(id, ctx, ct);

                if (!r.IsSuccess) return DXResult<bool>.Fail(r.Error!);

                if (r.Flow == DXFlow.SkipProcess) flow = DXFlow.SkipProcess;
                if (r.Flow == DXFlow.Stop) return DXResult<bool>.OkStop();

                result = r.Value;
            }

            if (flow != DXFlow.SkipProcess)
            {
                var typeName = DXUnitHelper.GetTypeName(typeof(T));

                result = coreRepo.IsItemExisting(typeName, id);
            }

            return flow switch
            {
                DXFlow.SkipProcess => DXResult<bool>.OkSkipProcess(result),
                DXFlow.Stop => DXResult<bool>.OkStop(result),
                _ => DXResult<bool>.OkContinue(result),
            };
        }

        public async Task<DXResult<bool>> IsUnitExistingAsync(string typeName, Guid id, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            if (getHandlerProvider.TryResolveType(typeName, out var modelType))
            {
                var inv = GetIsUnitExistingInvoker(modelType);

                var baseRes = await inv(this, id, ctx, ct);

                if (!baseRes.IsSuccess)
                    return DXResult<bool>.Fail(baseRes.Error!);

                return DXResult<bool>.Ok(baseRes.Value, baseRes.Flow);
            }
            else
            {
                var result = coreRepo.IsItemExisting(typeName, id);

                return DXResult<bool>.OkContinue(result);
            }
        }

        private static readonly ConcurrentDictionary<Type,
            Func<DXPipelineExecutor, Guid, DXHandlerBaseContext, CancellationToken, Task<DXResult<DXUnit?>>>> _getInvokers = new();

        private static readonly ConcurrentDictionary<Type,
          Func<DXPipelineExecutor, IEnumerable<Guid>, DXHandlerBaseContext, CancellationToken, Task<DXResult<IEnumerable<DXUnit>?>>>> _getItemsInvokers = new();

        private static readonly ConcurrentDictionary<Type,
            Func<DXPipelineExecutor, Guid, DXHandlerBaseContext, CancellationToken, Task<DXResult<bool>>>> _isUnitExistingInvokers = new();

        private static readonly ConcurrentDictionary<Type,
            Func<DXPipelineExecutor, DXUnit, DXHandlerBaseContext, CancellationToken, Task<DXResult<DXUnit>>>> _insertInvokers = new();

        private static readonly ConcurrentDictionary<Type,
            Func<DXPipelineExecutor, DXUnit, DXHandlerBaseContext, CancellationToken, Task<DXResult<DXUnit>>>> _updateInvokers = new();

        private static readonly ConcurrentDictionary<Type,
            Func<DXPipelineExecutor, DXUnit, DXHandlerBaseContext, CancellationToken, Task<DXResult<DXUnit>>>> _deleteInvokers = new();


        private static async Task<DXResult<DXUnit?>> InvokeTypedGet<T>(
            DXPipelineExecutor exec, Guid id, DXHandlerBaseContext ctx, CancellationToken ct) where T : DXUnit, new()
        {
            var r = await exec.GetAsync<T>(id, ctx, ct);
            return DXResult<DXUnit?>.MapFrom(r, r.Value);
        }

        private static async Task<DXResult<IEnumerable<DXUnit>?>> InvokeTypedGetItems<T>(
          DXPipelineExecutor exec, IEnumerable<Guid> ids, DXHandlerBaseContext ctx, CancellationToken ct) where T : DXUnit, new()
        {
            var r = await exec.GetItemsAsync<T>(ids, ctx, ct);
            return DXResult<IEnumerable<DXUnit>?>.MapFrom(r, r.Value);
        }

        private static async Task<DXResult<bool>> InvokeTypedIsUnitExisting<T>(
            DXPipelineExecutor exec, Guid id, DXHandlerBaseContext ctx, CancellationToken ct) where T : DXUnit, new()
        {
            var r = await exec.IsUnitExistingAsync<T>(id, ctx, ct);
            return DXResult<bool>.MapFrom(r, r.Value);
        }

        private static async Task<DXResult<DXUnit>> InvokeTypedInsert<T>(
            DXPipelineExecutor exec, DXUnit model, DXHandlerBaseContext ctx, CancellationToken ct) where T : DXUnit, new()
        {
            if (model is not T m) return DXResult<DXUnit>.Fail($"Wrong model type. Expected {typeof(T).Name}");
            var r = await exec.InsertAsync<T>(m, ctx, ct);
            return r.IsSuccess ? DXResult<DXUnit>.Ok(r.Value!, r.Flow) : DXResult<DXUnit>.Fail(r.Error!);
        }

        private static async Task<DXResult<DXUnit>> InvokeTypedUpdate<T>(
            DXPipelineExecutor exec, DXUnit model, DXHandlerBaseContext ctx, CancellationToken ct) where T : DXUnit, new()
        {
            if (model is not T m) return DXResult<DXUnit>.Fail($"Wrong model type. Expected {typeof(T).Name}");
            var r = await exec.UpdateAsync<T>(m, ctx, ct);
            return r.IsSuccess ? DXResult<DXUnit>.Ok(r.Value!, r.Flow) : DXResult<DXUnit>.Fail(r.Error!);
        }

        private static async Task<DXResult<DXUnit>> InvokeTypedDelete<T>(
            DXPipelineExecutor exec, DXUnit model, DXHandlerBaseContext ctx, CancellationToken ct) where T : DXUnit, new()
        {
            if (model is not T m) return DXResult<DXUnit>.Fail($"Wrong model type. Expected {typeof(T).Name}");
            var r = await exec.DeleteAsync<T>(m, ctx, ct);
            return r.IsSuccess ? DXResult<DXUnit>.Ok(r.Value!, r.Flow) : DXResult<DXUnit>.Fail(r.Error!);
        }

        private Func<DXPipelineExecutor, Guid, DXHandlerBaseContext, CancellationToken, Task<DXResult<DXUnit?>>> GetGetInvoker(Type modelType)
            => _getInvokers.GetOrAdd(modelType, static t =>
            {
                var mi = typeof(DXPipelineExecutor)
                    .GetMethod(nameof(InvokeTypedGet), BindingFlags.NonPublic | BindingFlags.Static)!
                    .MakeGenericMethod(t);

                return (Func<DXPipelineExecutor, Guid, DXHandlerBaseContext, CancellationToken, Task<DXResult<DXUnit?>>>)
                    Delegate.CreateDelegate(
                        typeof(Func<DXPipelineExecutor, Guid, DXHandlerBaseContext, CancellationToken, Task<DXResult<DXUnit?>>>),
                        mi);
            });

        private Func<DXPipelineExecutor, IEnumerable<Guid>, DXHandlerBaseContext, CancellationToken, Task<DXResult<IEnumerable<DXUnit>?>>> GetGetItemsInvoker(Type modelType)
           => _getItemsInvokers.GetOrAdd(modelType, static t =>
           {
               var mi = typeof(DXPipelineExecutor)
                   .GetMethod(nameof(InvokeTypedGetItems), BindingFlags.NonPublic | BindingFlags.Static)!
                   .MakeGenericMethod(t);

               return (Func<DXPipelineExecutor, IEnumerable<Guid>, DXHandlerBaseContext, CancellationToken, Task<DXResult<IEnumerable<DXUnit>?>>>)
                   Delegate.CreateDelegate(
                       typeof(Func<DXPipelineExecutor, IEnumerable<Guid>, DXHandlerBaseContext, CancellationToken, Task<DXResult<IEnumerable<DXUnit>?>>>),
                       mi);
           });

        private Func<DXPipelineExecutor, Guid, DXHandlerBaseContext, CancellationToken, Task<DXResult<bool>>> GetIsUnitExistingInvoker(Type modelType)
           => _isUnitExistingInvokers.GetOrAdd(modelType, static t =>
           {
               var mi = typeof(DXPipelineExecutor)
                   .GetMethod(nameof(InvokeTypedIsUnitExisting), BindingFlags.NonPublic | BindingFlags.Static)!
                   .MakeGenericMethod(t);

               return (Func<DXPipelineExecutor, Guid, DXHandlerBaseContext, CancellationToken, Task<DXResult<bool>>>)
                   Delegate.CreateDelegate(
                       typeof(Func<DXPipelineExecutor, Guid, DXHandlerBaseContext, CancellationToken, Task<DXResult<bool>>>),
                       mi);
           });


        private Func<DXPipelineExecutor, DXUnit, DXHandlerBaseContext, CancellationToken, Task<DXResult<DXUnit>>> GetInsertInvoker(Type modelType)
            => _insertInvokers.GetOrAdd(modelType, static t =>
            {
                var mi = typeof(DXPipelineExecutor)
                    .GetMethod(nameof(InvokeTypedInsert), BindingFlags.NonPublic | BindingFlags.Static)!
                    .MakeGenericMethod(t);

                return (Func<DXPipelineExecutor, DXUnit, DXHandlerBaseContext, CancellationToken, Task<DXResult<DXUnit>>>)
                    Delegate.CreateDelegate(
                        typeof(Func<DXPipelineExecutor, DXUnit, DXHandlerBaseContext, CancellationToken, Task<DXResult<DXUnit>>>),
                        mi);
            });

        private Func<DXPipelineExecutor, DXUnit, DXHandlerBaseContext, CancellationToken, Task<DXResult<DXUnit>>> GetUpdateInvoker(Type modelType)
            => _updateInvokers.GetOrAdd(modelType, static t =>
            {
                var mi = typeof(DXPipelineExecutor)
                    .GetMethod(nameof(InvokeTypedUpdate), BindingFlags.NonPublic | BindingFlags.Static)!
                    .MakeGenericMethod(t);

                return (Func<DXPipelineExecutor, DXUnit, DXHandlerBaseContext, CancellationToken, Task<DXResult<DXUnit>>>)
                    Delegate.CreateDelegate(
                        typeof(Func<DXPipelineExecutor, DXUnit, DXHandlerBaseContext, CancellationToken, Task<DXResult<DXUnit>>>),
                        mi);
            });

        private Func<DXPipelineExecutor, DXUnit, DXHandlerBaseContext, CancellationToken, Task<DXResult<DXUnit>>> GetDeleteInvoker(Type modelType)
            => _deleteInvokers.GetOrAdd(modelType, static t =>
            {
                var mi = typeof(DXPipelineExecutor)
                    .GetMethod(nameof(InvokeTypedDelete), BindingFlags.NonPublic | BindingFlags.Static)!
                    .MakeGenericMethod(t);

                return (Func<DXPipelineExecutor, DXUnit, DXHandlerBaseContext, CancellationToken, Task<DXResult<DXUnit>>>)
                    Delegate.CreateDelegate(
                        typeof(Func<DXPipelineExecutor, DXUnit, DXHandlerBaseContext, CancellationToken, Task<DXResult<DXUnit>>>),
                        mi);
            });


        public static void WarmUpInvokers(IEnumerable<Type> unitTypes)
        {
            foreach (var t in unitTypes)
            {
                _getInvokers.GetOrAdd(t, _ => MakeGet(t));
                _getItemsInvokers.GetOrAdd(t, _ => MakeGetItems(t));
                _isUnitExistingInvokers.GetOrAdd(t, _ => MakeIsUnitExisting(t));
                _insertInvokers.GetOrAdd(t, _ => MakeInsert(t));
                _updateInvokers.GetOrAdd(t, _ => MakeUpdate(t));
                _deleteInvokers.GetOrAdd(t, _ => MakeDelete(t));
            }

            static Func<DXPipelineExecutor, Guid, DXHandlerBaseContext, CancellationToken, Task<DXResult<DXUnit?>>> MakeGet(Type t)
                => (Func<DXPipelineExecutor, Guid, DXHandlerBaseContext, CancellationToken, Task<DXResult<DXUnit?>>>)
                   Delegate.CreateDelegate(
                     typeof(Func<DXPipelineExecutor, Guid, DXHandlerBaseContext, CancellationToken, Task<DXResult<DXUnit?>>>),
                     typeof(DXPipelineExecutor).GetMethod(nameof(InvokeTypedGet), BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(t));

            static Func<DXPipelineExecutor, IEnumerable<Guid>, DXHandlerBaseContext, CancellationToken, Task<DXResult<IEnumerable<DXUnit>?>>> MakeGetItems(Type t)
               => (Func<DXPipelineExecutor, IEnumerable<Guid>, DXHandlerBaseContext, CancellationToken, Task<DXResult<IEnumerable<DXUnit>?>>>)
                  Delegate.CreateDelegate(
                    typeof(Func<DXPipelineExecutor, IEnumerable<Guid>, DXHandlerBaseContext, CancellationToken, Task<DXResult<IEnumerable<DXUnit>?>>>),
                    typeof(DXPipelineExecutor).GetMethod(nameof(InvokeTypedGetItems), BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(t));

            static Func<DXPipelineExecutor, Guid, DXHandlerBaseContext, CancellationToken, Task<DXResult<bool>>> MakeIsUnitExisting(Type t)
                => (Func<DXPipelineExecutor, Guid, DXHandlerBaseContext, CancellationToken, Task<DXResult<bool>>>)
                Delegate.CreateDelegate(
                   typeof(Func<DXPipelineExecutor, Guid, DXHandlerBaseContext, CancellationToken, Task<DXResult<bool>>>),
                   typeof(DXPipelineExecutor).GetMethod(nameof(InvokeTypedIsUnitExisting), BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(t));

            static Func<DXPipelineExecutor, DXUnit, DXHandlerBaseContext, CancellationToken, Task<DXResult<DXUnit>>> MakeInsert(Type t)
                => (Func<DXPipelineExecutor, DXUnit, DXHandlerBaseContext, CancellationToken, Task<DXResult<DXUnit>>>)
                   Delegate.CreateDelegate(
                     typeof(Func<DXPipelineExecutor, DXUnit, DXHandlerBaseContext, CancellationToken, Task<DXResult<DXUnit>>>),
                     typeof(DXPipelineExecutor).GetMethod(nameof(InvokeTypedInsert), BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(t));

            static Func<DXPipelineExecutor, DXUnit, DXHandlerBaseContext, CancellationToken, Task<DXResult<DXUnit>>> MakeUpdate(Type t)
                => (Func<DXPipelineExecutor, DXUnit, DXHandlerBaseContext, CancellationToken, Task<DXResult<DXUnit>>>)
                   Delegate.CreateDelegate(
                     typeof(Func<DXPipelineExecutor, DXUnit, DXHandlerBaseContext, CancellationToken, Task<DXResult<DXUnit>>>),
                     typeof(DXPipelineExecutor).GetMethod(nameof(InvokeTypedUpdate), BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(t));

            static Func<DXPipelineExecutor, DXUnit, DXHandlerBaseContext, CancellationToken, Task<DXResult<DXUnit>>> MakeDelete(Type t)
                => (Func<DXPipelineExecutor, DXUnit, DXHandlerBaseContext, CancellationToken, Task<DXResult<DXUnit>>>)
                   Delegate.CreateDelegate(
                     typeof(Func<DXPipelineExecutor, DXUnit, DXHandlerBaseContext, CancellationToken, Task<DXResult<DXUnit>>>),
                     typeof(DXPipelineExecutor).GetMethod(nameof(InvokeTypedDelete), BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(t));
        }
    }
}

