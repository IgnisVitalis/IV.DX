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

        public async Task<DXResult<bool>> IsItemExistingAsync(Guid id, IDXHandlerContext ctx, CancellationToken ct)
        {
            return DXResult<bool>.OkSkipProcess(false);
        }

        public async Task<DXResult<DXRelationItemUnit>> BeforeInsertAsync(DXRelationItemUnit dxUnit, IDXHandlerContext ctx, CancellationToken ct)
        {
            genericRepo.AddDXRelation(dxUnit);

            return DXResult<DXRelationItemUnit>.OkSkipProcess(dxUnit);
        }

        public async Task<DXResult<DXRelationItemUnit>> BeforeUpdateAsync(DXRelationItemUnit dxUnit, IDXHandlerContext ctx, CancellationToken ct)
        {
            return DXResult<DXRelationItemUnit>.OkSkipProcess(dxUnit);
        }
    }
}