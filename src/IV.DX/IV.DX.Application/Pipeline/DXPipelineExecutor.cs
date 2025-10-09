using IV.DX.Application.Contracts.Pipeline;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Converters;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using System.Collections.Concurrent;
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
            Guid id, IDXHandlerContext ctx, CancellationToken ct) where T : DXUnit
        {
            var flow = DXFlow.Continue;
            T? dxUnit = default;

            foreach (var h in getHandlerProvider.GetBeforeGetHandlers<T>())
            {
                var r = await h.BeforeGetAsync(id, ctx, ct);
                if (!r.IsSuccess) return DXResult<T?>.Fail(r.Error!);

                dxUnit = r.Value;
                if (r.Flow == DXFlow.SkipProcess) flow = DXFlow.SkipProcess;
                if (r.Flow == DXFlow.Stop) return DXResult<T?>.OkStop(dxUnit);
            }

            if (flow != DXFlow.SkipProcess)
            {
                dxUnit = genericRepo.GetItem<T>(id);
                if (dxUnit is null) return DXResult<T?>.Fail($"DXUnit '{typeof(T).Name}:{id}' not found.");
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

        public async Task<DXResult<DXModel?>> GetAsync(
            string typeName, Guid id, IDXHandlerContext ctx, CancellationToken ct)
        {
            if (getHandlerProvider.TryResolveType(typeName, out var modelType))
            {
                var inv = GetGetInvoker(modelType);
                var baseRes = await inv(this, id, ctx, ct);

                if (!baseRes.IsSuccess)
                    return DXResult<DXModel?>.Fail(baseRes.Error!);

                var dxModel = baseRes.Value is null
                    ? null
                    : DXUnitHelper.ConvertToESQLModel(baseRes.Value);

                return DXResult<DXModel?>.Ok(dxModel, baseRes.Flow);
            }

            var dxModelRaw = coreRepo.GetItem(typeName, id);
            if (dxModelRaw is null) return DXResult<DXModel?>.Fail($"DXModel '{typeName}:{id}' not found.");

            return DXResult<DXModel?>.OkContinue(dxModelRaw);
        }

        public async Task<DXResult<T>> InsertAsync<T>(
            T dxUnit,
            IDXHandlerContext ctx,
            CancellationToken ct) where T : DXUnit
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

        public async Task<DXResult<DXModel>> InsertAsync(
            DXModel dxModel,
            IDXHandlerContext ctx,
            CancellationToken ct)
        {
            var typeName = dxModel.OwnSingleItem.ObjectInfo.ObjectName;

            if (string.IsNullOrWhiteSpace(typeName))
                return DXResult<DXModel>.Fail("Type name not found in payload.");

            if (insertHandlerProvider.TryResolveType(typeName, out var modelType))
            {
                DXUnit? dxUnit;
                try
                {
                    dxUnit = DXUnitHelper.CreateInstance(dxModel, modelType);
                }
                catch (Exception e)
                {
                    return DXResult<DXModel>.Fail($"Failed to deserialize DXUnit: {e.Message}");
                }

                if (dxUnit is null)
                    return DXResult<DXModel>.Fail("Failed to deserialize DXUnit.");

                var inv = GetInsertInvoker(modelType);
                var baseRes = await inv(this, dxUnit, ctx, ct);
                if (!baseRes.IsSuccess) return DXResult<DXModel>.Fail(baseRes.Error!);

                var dxModelResult = DXUnitHelper.ConvertToESQLModel(baseRes.Value!);
                return DXResult<DXModel>.Ok(dxModelResult, baseRes.Flow);
            }

            var id = coreRepo.Insert(dxModel);
            var saved = coreRepo.GetItem(typeName, id);

            if (saved is null) return DXResult<DXModel>.Fail("Inserted DXModel not found.");

            return DXResult<DXModel>.OkContinue(saved);
        }

        public async Task<DXResult<T>> UpdateAsync<T>(
            T dxUnit,
            IDXHandlerContext ctx,
            CancellationToken ct) where T : DXUnit
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

        public async Task<DXResult<DXModel>> UpdateAsync(
            DXModel dxModel,
            IDXHandlerContext ctx,
            CancellationToken ct)
        {
            var typeName = dxModel.OwnSingleItem.ObjectInfo.ObjectName;

            if (string.IsNullOrWhiteSpace(typeName))
                return DXResult<DXModel>.Fail("Type name not found in payload.");

            if (insertHandlerProvider.TryResolveType(typeName, out var modelType))
            {
                DXUnit? dxUnit;

                try
                {
                    dxUnit = DXUnitHelper.CreateInstance(dxModel, modelType);
                }
                catch (Exception e)
                {
                    return DXResult<DXModel>.Fail($"Failed to deserialize DXUnit: {e.Message}");
                }

                if (dxUnit is null)
                    return DXResult<DXModel>.Fail("Failed to deserialize DXUnit.");

                var inv = GetUpdateInvoker(modelType);
                var baseRes = await inv(this, dxUnit, ctx, ct);
                if (!baseRes.IsSuccess) return DXResult<DXModel>.Fail(baseRes.Error!);

                var dxModelResult = DXUnitHelper.ConvertToESQLModel(baseRes.Value!);
                return DXResult<DXModel>.Ok(dxModelResult, baseRes.Flow);
            }

            var id = coreRepo.Update(dxModel);
            var saved = coreRepo.GetItem(typeName, id);

            if (saved is null) return DXResult<DXModel>.Fail("Updated DXModel not found.");

            return DXResult<DXModel>.OkContinue(saved);
        }

        public async Task<DXResult<T>> DeleteAsync<T>(T dxUnit, IDXHandlerContext ctx, CancellationToken ct) where T : DXUnit
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

        public async Task<DXResult<DXModel>> DeleteAsync(DXModel dxModel, IDXHandlerContext ctx, CancellationToken ct)
        {
            var typeName = dxModel.OwnSingleItem.ObjectInfo.ObjectName;

            if (string.IsNullOrWhiteSpace(typeName))
                return DXResult<DXModel>.Fail("Type name not found in payload.");

            if (insertHandlerProvider.TryResolveType(typeName, out var modelType))
            {
                DXUnit? dxUnit;

                try
                {
                    dxUnit = DXUnitHelper.CreateInstance(dxModel, modelType);
                }
                catch (Exception e)
                {
                    return DXResult<DXModel>.Fail($"Failed to deserialize DXUnit: {e.Message}");
                }

                if (dxUnit is null)
                    return DXResult<DXModel>.Fail("Failed to deserialize DXUnit.");

                var inv = GetDeleteInvoker(modelType);
                var baseRes = await inv(this, dxUnit, ctx, ct);
                if (!baseRes.IsSuccess) return DXResult<DXModel>.Fail(baseRes.Error!);

                var dxModelResult = DXUnitHelper.ConvertToESQLModel(baseRes.Value!);
                return DXResult<DXModel>.Ok(dxModelResult, baseRes.Flow);
            }

            var result = coreRepo.Delete(dxModel.OwnSingleItem.ObjectInfo.ObjectName, dxModel.OwnSingleItem.Item.ID.Value);

            if (!result) return DXResult<DXModel>.Fail("DXModel isnot deleted.");

            return DXResult<DXModel>.OkContinue(dxModel);
        }

        // GET уже есть:
        private static readonly ConcurrentDictionary<Type,
            Func<DXPipelineExecutor, Guid, IDXHandlerContext, CancellationToken, Task<DXResult<DXUnit?>>>> _getInvokers = new();

        // INSERT / UPDATE / DELETE:
        private static readonly ConcurrentDictionary<Type,
            Func<DXPipelineExecutor, DXUnit, IDXHandlerContext, CancellationToken, Task<DXResult<DXUnit>>>> _insertInvokers = new();

        private static readonly ConcurrentDictionary<Type,
            Func<DXPipelineExecutor, DXUnit, IDXHandlerContext, CancellationToken, Task<DXResult<DXUnit>>>> _updateInvokers = new();

        private static readonly ConcurrentDictionary<Type,
            Func<DXPipelineExecutor, DXUnit, IDXHandlerContext, CancellationToken, Task<DXResult<DXUnit>>>> _deleteInvokers = new();

        // ---------- статические generic-хелперы, которые закрываются по Type ОДИН РАЗ ----------
        private static async Task<DXResult<DXUnit?>> InvokeTypedGet<T>(
            DXPipelineExecutor exec, Guid id, IDXHandlerContext ctx, CancellationToken ct) where T : DXUnit
        {
            var r = await exec.GetAsync<T>(id, ctx, ct);
            return DXResult<DXUnit?>.MapFrom(r, r.Value);
        }

        private static async Task<DXResult<DXUnit>> InvokeTypedInsert<T>(
            DXPipelineExecutor exec, DXUnit model, IDXHandlerContext ctx, CancellationToken ct) where T : DXUnit
        {
            if (model is not T m) return DXResult<DXUnit>.Fail($"Wrong model type. Expected {typeof(T).Name}");
            var r = await exec.InsertAsync<T>(m, ctx, ct);
            return r.IsSuccess ? DXResult<DXUnit>.Ok(r.Value!, r.Flow) : DXResult<DXUnit>.Fail(r.Error!);
        }

        private static async Task<DXResult<DXUnit>> InvokeTypedUpdate<T>(
            DXPipelineExecutor exec, DXUnit model, IDXHandlerContext ctx, CancellationToken ct) where T : DXUnit
        {
            if (model is not T m) return DXResult<DXUnit>.Fail($"Wrong model type. Expected {typeof(T).Name}");
            var r = await exec.UpdateAsync<T>(m, ctx, ct);
            return r.IsSuccess ? DXResult<DXUnit>.Ok(r.Value!, r.Flow) : DXResult<DXUnit>.Fail(r.Error!);
        }

        private static async Task<DXResult<DXUnit>> InvokeTypedDelete<T>(
            DXPipelineExecutor exec, DXUnit model, IDXHandlerContext ctx, CancellationToken ct) where T : DXUnit
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
                _insertInvokers.GetOrAdd(t, _ => MakeInsert(t));
                _updateInvokers.GetOrAdd(t, _ => MakeUpdate(t));
                _deleteInvokers.GetOrAdd(t, _ => MakeDelete(t));
            }

            static Func<DXPipelineExecutor, Guid, IDXHandlerContext, CancellationToken, Task<DXResult<DXUnit?>>> MakeGet(Type t)
                => (Func<DXPipelineExecutor, Guid, IDXHandlerContext, CancellationToken, Task<DXResult<DXUnit?>>>)
                   Delegate.CreateDelegate(
                     typeof(Func<DXPipelineExecutor, Guid, IDXHandlerContext, CancellationToken, Task<DXResult<DXUnit?>>>),
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
