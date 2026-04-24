using IV.DX.Application.Contracts.Handlers;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Application.Handlers
{
    internal class DXRelationItemUnitHandler(IDXUnitGenericRepository genericRepo) :
        IDXIsItemExistingHandler<DXRelationItemUnit>, IDXUniqueIsItemExistingHandler,
        IDXBeforeInsertHandler<DXRelationItemUnit>, IDXUniqueBeforeInsertHandler,
        IDXBeforeUpdateHandler<DXRelationItemUnit>, IDXUniqueBeforeUpdateHandler
    {
        public int BeforeOrder => 1;

        public Task<DXResult<bool>> IsItemExistingAsync(Guid id, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            return Task.FromResult(DXResult<bool>.OkSkipProcess(false));
        }

        public Task<DXResult<DXRelationItemUnit>> BeforeInsertAsync(DXRelationItemUnit dxUnit, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            genericRepo.AddDXRelation(dxUnit);

            return Task.FromResult(DXResult<DXRelationItemUnit>.OkSkipProcess(dxUnit));
        }

        public Task<DXResult<DXRelationItemUnit>> BeforeUpdateAsync(DXRelationItemUnit dxUnit, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            return Task.FromResult(DXResult<DXRelationItemUnit>.OkSkipProcess(dxUnit));
        }
    }
}