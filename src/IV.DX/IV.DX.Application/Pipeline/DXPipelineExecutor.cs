using IV.DX.Application.Contracts.Pipeline;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Converters;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace IV.DX.Application.Pipeline
{
    internal class DXPipelineExecutor(
        IDXCoreRepository coreRepo,
        IDXGenericRepository genericRepo,
        IDXUnitGetHandlerProvider getHandlerProvider,
        IDXUnitInsertHandlerProvider insertHandlerProvider,
        IDXUnitUpdateHandlerProvider updateHandlerProvider,
        IDXUnitDeleteHandlerProvider deleteHandlerProvider)
        : IDXPipelineExecutor
    {
        public async Task<DXResult<T?>> GetAsync<T>(
            Guid id, IDXHandlerContext ctx, CancellationToken ct) where T : DXUnit, new()
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
                dxUnit = genericRepo.GetItem<T>(id);
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
            string typeName, Guid id, IDXHandlerContext ctx, CancellationToken ct)
        {
            if (getHandlerProvider.TryResolveType(typeName, out var modelType))
            {
                var inv = GetGetInvoker(modelType);
                var baseRes = await inv(this, id, ctx, ct);

                if (!baseRes.IsSuccess)
                    return DXResult<JObject?>.Fail(baseRes.Error!);

                var dxModel = baseRes.Value is null
                    ? null
                    : DXUnitHelper.ConvertToJObject(baseRes.Value);

                return DXResult<JObject?>.Ok(dxModel, baseRes.Flow);
            }

            var dxModelRaw = coreRepo.GetItem(typeName, id)?.ConvertToJObject();

            if (dxModelRaw is null) return DXResult<JObject?>.NotFound();

            return DXResult<JObject?>.OkContinue(dxModelRaw);
        }

        public async Task<DXResult<T>> InsertAsync<T>(
            T dxUnit,
            IDXHandlerContext ctx,
            CancellationToken ct) where T : DXUnit, new()
        {
            var flow = DXFlow.Continue;

            foreach (var h in insertHandlerProvider.GetBeforeInsertHandlers<T>())
            {
                var r = await h.BeforeInsertAsync(dxUnit, ctx, ct);
                if (!r.IsSuccess) return DXResult<T>.Fail(r.Error!);

                dxUnit = r.Value!;
                if (r.Flow == DXFlow.SkipProcess) flow = DXFlow.SkipProcess;
                if (r.Flow == DXFlow.Stop) return DXResult<T>.OkStop(dxUnit);
            }

            if (flow != DXFlow.SkipProcess)
            {
                var id = genericRepo.Insert(dxUnit);
                var reloaded = genericRepo.GetItem<T>(id);

                if (reloaded is null) return DXResult<T>.Fail("Inserted entity not found.");
                dxUnit = reloaded;
            }

            foreach (var h in insertHandlerProvider.GetAfterInsertHandlers<T>())
            {
                var r = await h.AfterInsertAsync(dxUnit, ctx, ct);
                if (!r.IsSuccess) return DXResult<T>.Fail(r.Error!);
            }

            return flow switch
            {
                DXFlow.SkipProcess => DXResult<T>.OkSkipProcess(dxUnit),
                DXFlow.Stop => DXResult<T>.OkStop(dxUnit),
                _ => DXResult<T>.OkContinue(dxUnit),
            };
        }

        public async Task<DXResult<JObject>> InsertAsync(
            JObject jObject,
            IDXHandlerContext ctx,
            CancellationToken ct)
        {
            var typeName = DXUnitHelper.GetTypeName(jObject);

            if (string.IsNullOrWhiteSpace(typeName))
                return DXResult<JObject>.Fail("Type name not found in payload.");

            if (insertHandlerProvider.TryResolveType(typeName, out var modelType))
            {
                DXUnit? dxUnit;
                try
                {
                    dxUnit = DXUnitHelper.CreateInstance(jObject, modelType);
                }
                catch (Exception e)
                {
                    return DXResult<JObject>.Fail($"Failed to deserialize DXUnit: {e.Message}");
                }

                if (dxUnit is null)
                    return DXResult<JObject>.Fail("Failed to deserialize DXUnit.");

                var inv = GetInsertInvoker(modelType);
                var baseRes = await inv(this, dxUnit, ctx, ct);
                if (!baseRes.IsSuccess) return DXResult<JObject>.Fail(baseRes.Error!);

                var dxModelResult = DXUnitHelper.ConvertToJObject(baseRes.Value!);
                return DXResult<JObject>.Ok(dxModelResult, baseRes.Flow);
            }
            else
            {
                var dxModel = DXModel.CreateInstance(jObject);
                DXModelDefinition modelDefinition = DXModelDefinitionHelper.GetESQLModelDefinition(dxModel);

                var id = coreRepo.Insert(dxModel);
                var saved = coreRepo.GetItem(modelDefinition, id, Kernel.Enums.DXLoadingType.Full);

                if (saved is null) return DXResult<JObject>.Fail("Inserted DXModel not found.");

                return DXResult<JObject>.OkContinue(saved.ConvertToJObject());
            }
        }

        public async Task<DXResult<T>> UpdateAsync<T>(
            T dxUnit,
            IDXHandlerContext ctx,
            CancellationToken ct) where T : DXUnit, new()
        {
            var flow = DXFlow.Continue;

            foreach (var h in updateHandlerProvider.GetBeforeUpdateHandlers<T>())
            {
                var r = await h.BeforeUpdateAsync(dxUnit, ctx, ct);
                if (!r.IsSuccess) return DXResult<T>.Fail(r.Error!);

                dxUnit = r.Value!;
                if (r.Flow == DXFlow.SkipProcess) flow = DXFlow.SkipProcess;
                if (r.Flow == DXFlow.Stop) return DXResult<T>.OkStop(dxUnit);
            }

            if (flow != DXFlow.SkipProcess)
            {
                var id = genericRepo.Update(dxUnit);
                var reloaded = genericRepo.GetItem<T>(id);

                if (reloaded is null) return DXResult<T>.Fail("Inserted entity not found.");
                dxUnit = reloaded;
            }

            foreach (var h in updateHandlerProvider.GetAfterUpdateHandlers<T>())
            {
                var r = await h.AfterUpdateAsync(dxUnit, ctx, ct);
                if (!r.IsSuccess) return DXResult<T>.Fail(r.Error!);
            }

            return flow switch
            {
                DXFlow.SkipProcess => DXResult<T>.OkSkipProcess(dxUnit),
                DXFlow.Stop => DXResult<T>.OkStop(dxUnit),
                _ => DXResult<T>.OkContinue(dxUnit),
            };
        }

        public async Task<DXResult<JObject>> UpdateAsync(
            JObject jObject,
            IDXHandlerContext ctx,
            CancellationToken ct)
        {
            var typeName = DXUnitHelper.GetTypeName(jObject);

            if (string.IsNullOrWhiteSpace(typeName))
                return DXResult<JObject>.Fail("Type name not found in payload.");

            if (insertHandlerProvider.TryResolveType(typeName, out var modelType))
            {
                DXUnit? dxUnit;

                try
                {
                    dxUnit = DXUnitHelper.CreateInstance(jObject, modelType);
                }
                catch (Exception e)
                {
                    return DXResult<JObject>.Fail($"Failed to deserialize DXUnit: {e.Message}");
                }

                if (dxUnit is null)
                    return DXResult<JObject>.Fail("Failed to deserialize DXUnit.");

                var inv = GetUpdateInvoker(modelType);
                var baseRes = await inv(this, dxUnit, ctx, ct);
                if (!baseRes.IsSuccess) return DXResult<JObject>.Fail(baseRes.Error!);

                var dxModelResult = DXUnitHelper.ConvertToJObject(baseRes.Value!);
                return DXResult<JObject>.Ok(dxModelResult, baseRes.Flow);
            }
            else
            {
                var dxModel = DXModel.CreateInstance(jObject);
                DXModelDefinition modelDefinition = DXModelDefinitionHelper.GetESQLModelDefinition(dxModel);

                var id = coreRepo.Update(dxModel);
                var saved = coreRepo.GetItem(modelDefinition, id, Kernel.Enums.DXLoadingType.Full);

                if (saved is null) return DXResult<JObject>.Fail("Updated DXModel not found.");

                return DXResult<JObject>.OkContinue(saved.ConvertToJObject());
            }
        }

        public async Task<DXResult<T>> DeleteAsync<T>(T dxUnit, IDXHandlerContext ctx, CancellationToken ct) where T : DXUnit, new()
        {
            var flow = DXFlow.Continue;

            foreach (var h in deleteHandlerProvider.GetBeforeDeleteHandlers<T>())
            {
                var r = await h.BeforeDeleteAsync(dxUnit, ctx, ct);
                if (!r.IsSuccess) return DXResult<T>.Fail(r.Error!);

                dxUnit = r.Value!;
                if (r.Flow == DXFlow.SkipProcess) flow = DXFlow.SkipProcess;
                if (r.Flow == DXFlow.Stop) return DXResult<T>.OkStop(dxUnit);
            }

            if (flow != DXFlow.SkipProcess)
            {
                var result = genericRepo.Delete(dxUnit);

                if (!result) return DXResult<T>.Fail("Inserted entity not found.");
            }

            foreach (var h in deleteHandlerProvider.GetAfterDeleteHandlers<T>())
            {
                var r = await h.AfterDeleteAsync(dxUnit, ctx, ct);
                if (!r.IsSuccess) return DXResult<T>.Fail(r.Error!);
            }

            return flow switch
            {
                DXFlow.SkipProcess => DXResult<T>.OkSkipProcess(dxUnit),
                DXFlow.Stop => DXResult<T>.OkStop(dxUnit),
                _ => DXResult<T>.OkContinue(dxUnit),
            };
        }

        public async Task<DXResult<JObject>> DeleteAsync(JObject jObject, IDXHandlerContext ctx, CancellationToken ct)
        {
            var typeName = DXUnitHelper.GetTypeName(jObject);

            if (string.IsNullOrWhiteSpace(typeName))
                return DXResult<JObject>.Fail("Type name not found in payload.");

            if (insertHandlerProvider.TryResolveType(typeName, out var modelType))
            {
                DXUnit? dxUnit;

                try
                {
                    dxUnit = DXUnitHelper.CreateInstance(jObject, modelType);
                }
                catch (Exception e)
                {
                    return DXResult<JObject>.Fail($"Failed to deserialize DXUnit: {e.Message}");
                }

                if (dxUnit is null)
                    return DXResult<JObject>.Fail("Failed to deserialize DXUnit.");

                var inv = GetDeleteInvoker(modelType);
                var baseRes = await inv(this, dxUnit, ctx, ct);
                if (!baseRes.IsSuccess) return DXResult<JObject>.Fail(baseRes.Error!);

                var dxModelResult = DXUnitHelper.ConvertToESQLModel(baseRes.Value!);

                return DXResult<JObject>.Ok(dxModelResult.ConvertToJObject(), baseRes.Flow);
            }
            else
            {
                var result = coreRepo.Delete(typeName, DXUnitHelper.GetID(jObject));

                if (!result) return DXResult<JObject>.Fail("DXModel isnot deleted.");

                return DXResult<JObject>.OkContinue(jObject);
            }
        }

        public async Task<DXResult<IEnumerable<T>?>> GetItemsAsync<T>(
            IEnumerable<Guid> ids,
            IDXHandlerContext ctx,
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
                dxUnits = genericRepo.GetItems<T>(ids);

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
            IDXHandlerContext ctx,
            CancellationToken ct) where T : DXUnit, new()
        {
            var typeName = AttributeReader.GetESQLObjectTypeName(typeof(T));
            var ids = coreRepo.GetItemIDs(typeName, query);

            return await GetItemsAsync<T>(ids, ctx, ct);
        }

        public async Task<DXResult<IEnumerable<T>?>> GetItemsAsync<T>(
            IDXHandlerContext ctx,
            CancellationToken ct) where T : DXUnit, new()
        {
            return await GetItemsAsync<T>(string.Empty, ctx, ct);
        }

        public async Task<DXResult<IEnumerable<JObject>?>> GetItemsAsync(
            string typeName,
            IEnumerable<Guid> ids,
            IDXHandlerContext ctx,
            CancellationToken ct)
        {
            if (getHandlerProvider.TryResolveType(typeName, out var modelType))
            {
                var inv = GetGetItemsInvoker(modelType);

                var baseRes = await inv(this, ids, ctx, ct);

                if (!baseRes.IsSuccess)
                    return DXResult<IEnumerable<JObject>?>.Fail(baseRes.Error!);

                var dxModels = baseRes.Value is null
                    ? null
                    : baseRes.Value.Select(x => DXUnitHelper.ConvertToJObject(x)).ToList();

                return DXResult<IEnumerable<JObject>?>.Ok(dxModels, baseRes.Flow);
            }

            var result = coreRepo.GetItems(typeName, ids);

            var dxModelsRaw = result.Select(x => x.ConvertToJObject()).ToList();

            if (dxModelsRaw is null || dxModelsRaw.Count() == 0)
                return DXResult<IEnumerable<JObject>?>.NotFound();

            return DXResult<IEnumerable<JObject>?>.OkContinue(dxModelsRaw);
        }

        public async Task<DXResult<IEnumerable<JObject>?>> GetItemsAsync(
            string typeName,
            string query,
            IDXHandlerContext ctx,
            CancellationToken ct)
        {
            var ids = coreRepo.GetItemIDs(typeName, query);

            return await GetItemsAsync(typeName, ids, ctx, ct);
        }

        public async Task<DXResult<IEnumerable<JObject>?>> GetItemsAsync(
            string typeName,
            IDXHandlerContext ctx,
            CancellationToken ct)
        {
            return await GetItemsAsync(typeName, string.Empty, ctx, ct);
        }

        private static readonly ConcurrentDictionary<Type,
            Func<DXPipelineExecutor, Guid, IDXHandlerContext, CancellationToken, Task<DXResult<DXUnit?>>>> _getInvokers = new();

        private static readonly ConcurrentDictionary<Type,
          Func<DXPipelineExecutor, IEnumerable<Guid>, IDXHandlerContext, CancellationToken, Task<DXResult<IEnumerable<DXUnit>?>>>> _getItemsInvokers = new();

        private static readonly ConcurrentDictionary<Type,
            Func<DXPipelineExecutor, DXUnit, IDXHandlerContext, CancellationToken, Task<DXResult<DXUnit>>>> _insertInvokers = new();

        private static readonly ConcurrentDictionary<Type,
            Func<DXPipelineExecutor, DXUnit, IDXHandlerContext, CancellationToken, Task<DXResult<DXUnit>>>> _updateInvokers = new();

        private static readonly ConcurrentDictionary<Type,
            Func<DXPipelineExecutor, DXUnit, IDXHandlerContext, CancellationToken, Task<DXResult<DXUnit>>>> _deleteInvokers = new();


        private static async Task<DXResult<DXUnit?>> InvokeTypedGet<T>(
            DXPipelineExecutor exec, Guid id, IDXHandlerContext ctx, CancellationToken ct) where T : DXUnit, new()
        {
            var r = await exec.GetAsync<T>(id, ctx, ct);
            return DXResult<DXUnit?>.MapFrom(r, r.Value);
        }

        private static async Task<DXResult<IEnumerable<DXUnit>?>> InvokeTypedGetItems<T>(
          DXPipelineExecutor exec, IEnumerable<Guid> ids, IDXHandlerContext ctx, CancellationToken ct) where T : DXUnit, new()
        {
            var r = await exec.GetItemsAsync<T>(ids, ctx, ct);
            return DXResult<IEnumerable<DXUnit>?>.MapFrom(r, r.Value);
        }

        private static async Task<DXResult<DXUnit>> InvokeTypedInsert<T>(
            DXPipelineExecutor exec, DXUnit model, IDXHandlerContext ctx, CancellationToken ct) where T : DXUnit, new()
        {
            if (model is not T m) return DXResult<DXUnit>.Fail($"Wrong model type. Expected {typeof(T).Name}");
            var r = await exec.InsertAsync<T>(m, ctx, ct);
            return r.IsSuccess ? DXResult<DXUnit>.Ok(r.Value!, r.Flow) : DXResult<DXUnit>.Fail(r.Error!);
        }

        private static async Task<DXResult<DXUnit>> InvokeTypedUpdate<T>(
            DXPipelineExecutor exec, DXUnit model, IDXHandlerContext ctx, CancellationToken ct) where T : DXUnit, new()
        {
            if (model is not T m) return DXResult<DXUnit>.Fail($"Wrong model type. Expected {typeof(T).Name}");
            var r = await exec.UpdateAsync<T>(m, ctx, ct);
            return r.IsSuccess ? DXResult<DXUnit>.Ok(r.Value!, r.Flow) : DXResult<DXUnit>.Fail(r.Error!);
        }

        private static async Task<DXResult<DXUnit>> InvokeTypedDelete<T>(
            DXPipelineExecutor exec, DXUnit model, IDXHandlerContext ctx, CancellationToken ct) where T : DXUnit, new()
        {
            if (model is not T m) return DXResult<DXUnit>.Fail($"Wrong model type. Expected {typeof(T).Name}");
            var r = await exec.DeleteAsync<T>(m, ctx, ct);
            return r.IsSuccess ? DXResult<DXUnit>.Ok(r.Value!, r.Flow) : DXResult<DXUnit>.Fail(r.Error!);
        }

        private Func<DXPipelineExecutor, Guid, IDXHandlerContext, CancellationToken, Task<DXResult<DXUnit?>>> GetGetInvoker(Type modelType)
            => _getInvokers.GetOrAdd(modelType, static t =>
            {
                var mi = typeof(DXPipelineExecutor)
                    .GetMethod(nameof(InvokeTypedGet), BindingFlags.NonPublic | BindingFlags.Static)!
                    .MakeGenericMethod(t);

                return (Func<DXPipelineExecutor, Guid, IDXHandlerContext, CancellationToken, Task<DXResult<DXUnit?>>>)
                    Delegate.CreateDelegate(
                        typeof(Func<DXPipelineExecutor, Guid, IDXHandlerContext, CancellationToken, Task<DXResult<DXUnit?>>>),
                        mi);
            });

        private Func<DXPipelineExecutor, IEnumerable<Guid>, IDXHandlerContext, CancellationToken, Task<DXResult<IEnumerable<DXUnit>?>>> GetGetItemsInvoker(Type modelType)
           => _getItemsInvokers.GetOrAdd(modelType, static t =>
           {
               var mi = typeof(DXPipelineExecutor)
                   .GetMethod(nameof(InvokeTypedGetItems), BindingFlags.NonPublic | BindingFlags.Static)!
                   .MakeGenericMethod(t);

               return (Func<DXPipelineExecutor, IEnumerable<Guid>, IDXHandlerContext, CancellationToken, Task<DXResult<IEnumerable<DXUnit>?>>>)
                   Delegate.CreateDelegate(
                       typeof(Func<DXPipelineExecutor, IEnumerable<Guid>, IDXHandlerContext, CancellationToken, Task<DXResult<IEnumerable<DXUnit>?>>>),
                       mi);
           });

        private Func<DXPipelineExecutor, DXUnit, IDXHandlerContext, CancellationToken, Task<DXResult<DXUnit>>> GetInsertInvoker(Type modelType)
            => _insertInvokers.GetOrAdd(modelType, static t =>
            {
                var mi = typeof(DXPipelineExecutor)
                    .GetMethod(nameof(InvokeTypedInsert), BindingFlags.NonPublic | BindingFlags.Static)!
                    .MakeGenericMethod(t);

                return (Func<DXPipelineExecutor, DXUnit, IDXHandlerContext, CancellationToken, Task<DXResult<DXUnit>>>)
                    Delegate.CreateDelegate(
                        typeof(Func<DXPipelineExecutor, DXUnit, IDXHandlerContext, CancellationToken, Task<DXResult<DXUnit>>>),
                        mi);
            });

        private Func<DXPipelineExecutor, DXUnit, IDXHandlerContext, CancellationToken, Task<DXResult<DXUnit>>> GetUpdateInvoker(Type modelType)
            => _updateInvokers.GetOrAdd(modelType, static t =>
            {
                var mi = typeof(DXPipelineExecutor)
                    .GetMethod(nameof(InvokeTypedUpdate), BindingFlags.NonPublic | BindingFlags.Static)!
                    .MakeGenericMethod(t);

                return (Func<DXPipelineExecutor, DXUnit, IDXHandlerContext, CancellationToken, Task<DXResult<DXUnit>>>)
                    Delegate.CreateDelegate(
                        typeof(Func<DXPipelineExecutor, DXUnit, IDXHandlerContext, CancellationToken, Task<DXResult<DXUnit>>>),
                        mi);
            });

        private Func<DXPipelineExecutor, DXUnit, IDXHandlerContext, CancellationToken, Task<DXResult<DXUnit>>> GetDeleteInvoker(Type modelType)
            => _deleteInvokers.GetOrAdd(modelType, static t =>
            {
                var mi = typeof(DXPipelineExecutor)
                    .GetMethod(nameof(InvokeTypedDelete), BindingFlags.NonPublic | BindingFlags.Static)!
                    .MakeGenericMethod(t);

                return (Func<DXPipelineExecutor, DXUnit, IDXHandlerContext, CancellationToken, Task<DXResult<DXUnit>>>)
                    Delegate.CreateDelegate(
                        typeof(Func<DXPipelineExecutor, DXUnit, IDXHandlerContext, CancellationToken, Task<DXResult<DXUnit>>>),
                        mi);
            });


        public static void WarmUpInvokers(IEnumerable<Type> unitTypes)
        {
            foreach (var t in unitTypes)
            {
                _getInvokers.GetOrAdd(t, _ => MakeGet(t));
                _getItemsInvokers.GetOrAdd(t, _ => MakeGetItems(t));
                _insertInvokers.GetOrAdd(t, _ => MakeInsert(t));
                _updateInvokers.GetOrAdd(t, _ => MakeUpdate(t));
                _deleteInvokers.GetOrAdd(t, _ => MakeDelete(t));
            }

            static Func<DXPipelineExecutor, Guid, IDXHandlerContext, CancellationToken, Task<DXResult<DXUnit?>>> MakeGet(Type t)
                => (Func<DXPipelineExecutor, Guid, IDXHandlerContext, CancellationToken, Task<DXResult<DXUnit?>>>)
                   Delegate.CreateDelegate(
                     typeof(Func<DXPipelineExecutor, Guid, IDXHandlerContext, CancellationToken, Task<DXResult<DXUnit?>>>),
                     typeof(DXPipelineExecutor).GetMethod(nameof(InvokeTypedGet), BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(t));


            static Func<DXPipelineExecutor, IEnumerable<Guid>, IDXHandlerContext, CancellationToken, Task<DXResult<IEnumerable<DXUnit>?>>> MakeGetItems(Type t)
               => (Func<DXPipelineExecutor, IEnumerable<Guid>, IDXHandlerContext, CancellationToken, Task<DXResult<IEnumerable<DXUnit>?>>>)
                  Delegate.CreateDelegate(
                    typeof(Func<DXPipelineExecutor, IEnumerable<Guid>, IDXHandlerContext, CancellationToken, Task<DXResult<IEnumerable<DXUnit>?>>>),
                    typeof(DXPipelineExecutor).GetMethod(nameof(InvokeTypedGet), BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(t));

            static Func<DXPipelineExecutor, DXUnit, IDXHandlerContext, CancellationToken, Task<DXResult<DXUnit>>> MakeInsert(Type t)
                => (Func<DXPipelineExecutor, DXUnit, IDXHandlerContext, CancellationToken, Task<DXResult<DXUnit>>>)
                   Delegate.CreateDelegate(
                     typeof(Func<DXPipelineExecutor, DXUnit, IDXHandlerContext, CancellationToken, Task<DXResult<DXUnit>>>),
                     typeof(DXPipelineExecutor).GetMethod(nameof(InvokeTypedInsert), BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(t));

            static Func<DXPipelineExecutor, DXUnit, IDXHandlerContext, CancellationToken, Task<DXResult<DXUnit>>> MakeUpdate(Type t)
                => (Func<DXPipelineExecutor, DXUnit, IDXHandlerContext, CancellationToken, Task<DXResult<DXUnit>>>)
                   Delegate.CreateDelegate(
                     typeof(Func<DXPipelineExecutor, DXUnit, IDXHandlerContext, CancellationToken, Task<DXResult<DXUnit>>>),
                     typeof(DXPipelineExecutor).GetMethod(nameof(InvokeTypedUpdate), BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(t));

            static Func<DXPipelineExecutor, DXUnit, IDXHandlerContext, CancellationToken, Task<DXResult<DXUnit>>> MakeDelete(Type t)
                => (Func<DXPipelineExecutor, DXUnit, IDXHandlerContext, CancellationToken, Task<DXResult<DXUnit>>>)
                   Delegate.CreateDelegate(
                     typeof(Func<DXPipelineExecutor, DXUnit, IDXHandlerContext, CancellationToken, Task<DXResult<DXUnit>>>),
                     typeof(DXPipelineExecutor).GetMethod(nameof(InvokeTypedDelete), BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(t));
        }
    }
}
