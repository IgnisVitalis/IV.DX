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

        private static readonly ConcurrentDictionary<Type,
            Func<DXPipelineExecutor, Guid, IDXHandlerContext, CancellationToken, Task<DXResult<DXUnit?>>>>
            _getInvokers = new();

        private static async Task<DXResult<DXUnit?>> InvokeTypedGet<T>(
            DXPipelineExecutor exec, Guid id, IDXHandlerContext ctx, CancellationToken ct)
            where T : DXUnit
        {
            var r = await exec.GetAsync<T>(id, ctx, ct);
            return DXResult<DXUnit?>.MapFrom(r, r.Value);
        }

        private Func<DXPipelineExecutor, Guid, IDXHandlerContext, CancellationToken, Task<DXResult<DXUnit?>>>
            GetGetInvoker(Type modelType)
            => _getInvokers.GetOrAdd(modelType, BuildGetInvoker);
              
        private static Func<DXPipelineExecutor, Guid, IDXHandlerContext, CancellationToken, Task<DXResult<DXUnit?>>>
            BuildGetInvoker(Type t)
        {
            var mi = typeof(DXPipelineExecutor)
                .GetMethod(nameof(InvokeTypedGet), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(t);
         
            return (Func<DXPipelineExecutor, Guid, IDXHandlerContext, CancellationToken, Task<DXResult<DXUnit?>>>)
                Delegate.CreateDelegate(
                    typeof(Func<DXPipelineExecutor, Guid, IDXHandlerContext, CancellationToken, Task<DXResult<DXUnit?>>>),
                    mi);
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

                var typed = await InsertAsync(dxUnit, ctx, ct);
                if (!typed.IsSuccess) return DXResult<DXModel>.Fail(typed.Error!);

                var dxModelResult = DXUnitHelper.ConvertToESQLModel(typed.Value!);

                return DXResult<DXModel>.Ok(dxModelResult, typed.Flow);
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

                var typed = await UpdateAsync(dxUnit, ctx, ct);
                if (!typed.IsSuccess) return DXResult<DXModel>.Fail(typed.Error!);

                var dxModelResult = DXUnitHelper.ConvertToESQLModel(typed.Value!);

                return DXResult<DXModel>.Ok(dxModelResult, typed.Flow);
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

                var typed = await DeleteAsync(dxUnit, ctx, ct);
                if (!typed.IsSuccess) return DXResult<DXModel>.Fail(typed.Error!);

                var dxModelResult = DXUnitHelper.ConvertToESQLModel(typed.Value!);

                return DXResult<DXModel>.Ok(dxModelResult, typed.Flow);
            }

            var result = coreRepo.Delete(dxModel.OwnSingleItem.ObjectInfo.ObjectName, dxModel.OwnSingleItem.Item.ID.Value);

            if (!result) return DXResult<DXModel>.Fail("DXModel isnot deleted.");

            return DXResult<DXModel>.OkContinue(dxModel);
        }       
    }
}
