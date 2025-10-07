using IV.DX.Application.Contracts.Handlers;
using IV.DX.Application.Contracts.Pipeline;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Application.Pipeline
{
    internal class DXPipelineExecutor(IDXCoreRepository coreRepo, IDXGenericRepository genericRepo) : IDXPipelineExecutor
    {
        public async Task<DXResult<T?>> GetAsync<T>(
            Guid id,
            IEnumerable<IDXBeforeGet<T>> befores,
            IEnumerable<IDXAfterGet<T>> afters,
            IDXHandlerContext ctx,
            CancellationToken ct) where T : DXUnit
        {
            foreach (var b in befores.OrderBy(x => x.BeforeOrder))
            {
                var r = await b.BeforeGetAsync(id, ctx, ct);
                if (!r.IsSuccess) return DXResult<T?>.Fail(r.Error!);
            }

            var dxUnit = genericRepo.GetItem<T>(id);

            foreach (var a in afters.OrderBy(x => x.AfterOrder))
            {
                var r = await a.AfterGetAsync(dxUnit, ctx, ct);
                if (!r.IsSuccess) return DXResult<T?>.Fail(r.Error!);
            }

            return DXResult<T?>.OkContinue(dxUnit);
        }

        public async Task<DXResult<T>> InsertAsync<T>(
            T model,
            IEnumerable<IDXBeforeInsert<T>> befores,
            IEnumerable<IDXAfterInsert<T>> afters,
            IDXHandlerContext ctx,
            CancellationToken ct) where T : DXUnit
        {
            foreach (var b in befores.OrderBy(x => x.BeforeOrder))
            {
                var r = await b.BeforeInsertAsync(model, ctx, ct);
                if (!r.IsSuccess) return DXResult<T>.Fail(r.Error!);
                model = r.Value!;
            }

            var id = genericRepo.Insert(model);
            var dxUnit = genericRepo.GetItem<T>(id);

            foreach (var a in afters.OrderBy(x => x.AfterOrder))
            {
                var r = await a.AfterInsertAsync(dxUnit, ctx, ct);
                if (!r.IsSuccess) return DXResult<T>.Fail(r.Error!);
            }

            return DXResult<T>.OkContinue(dxUnit);
        }

        public async Task<DXResult<T>> UpdateAsync<T>(
            T model,
            IEnumerable<IDXBeforeUpdate<T>> befores,
            IEnumerable<IDXAfterUpdate<T>> afters,
            IDXHandlerContext ctx,
            CancellationToken ct) where T : DXUnit
        {
            foreach (var b in befores.OrderBy(x => x.BeforeOrder))
            {
                var r = await b.BeforeUpdateAsync(model, ctx, ct);
                if (!r.IsSuccess) return DXResult<T>.Fail(r.Error!);
                model = r.Value!;
            }

            var id = genericRepo.Update(model);
            var dxUnit = genericRepo.GetItem<T>(id);

            foreach (var a in afters.OrderBy(x => x.AfterOrder))
            {
                var r = await a.AfterUpdateAsync(dxUnit, ctx, ct);
                if (!r.IsSuccess) return DXResult<T>.Fail(r.Error!);
            }

            return DXResult<T>.OkContinue(dxUnit);
        }

        public async Task<DXResult> DeleteAsync<T>(
            Guid id,
            IEnumerable<IDXBeforeDelete<T>> befores,
            IEnumerable<IDXAfterDelete<T>> afters,
            IDXHandlerContext ctx,
            CancellationToken ct) where T : DXUnit
        {
            foreach (var b in befores.OrderBy(x => x.BeforeOrder))
            {
                var r = await b.BeforeDeleteAsync(id, ctx, ct);
                if (!r.IsSuccess) return DXResult.Fail(r.Error!);
            }

            var typeName = AttributeReader.GetESQLObjectTypeName(typeof(T));
            coreRepo.Delete(typeName, id);

            foreach (var a in afters.OrderBy(x => x.AfterOrder))
            {
                var r = await a.AfterDeleteAsync(id, ctx, ct);
                if (!r.IsSuccess) return DXResult.Fail(r.Error!);
            }

            return DXResult.Ok();
        }
    }
}
