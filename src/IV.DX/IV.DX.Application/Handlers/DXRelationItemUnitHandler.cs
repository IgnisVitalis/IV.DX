using IV.DX.Application.Contracts.Handlers;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Application.Handlers
{
    internal class DXRelationItemUnitHandler(IDXGenericRepository genericRepo) :
        IDXBeforeInsert<DXRelationItemUnit>,
        IDXBeforeUpdate<DXRelationItemUnit>
    {
        public int BeforeOrder => 1;

        public Task<DXResult<DXRelationItemUnit>> BeforeInsertAsync(DXRelationItemUnit dxUnit, IDXHandlerContext ctx, CancellationToken ct)
        {
            genericRepo.AddRelation(dxUnit);

            return Task.Run(() => DXResult<DXRelationItemUnit>.OkSkipProcess(dxUnit));
        }

        public Task<DXResult<DXRelationItemUnit>> BeforeUpdateAsync(DXRelationItemUnit dxUnit, IDXHandlerContext ctx, CancellationToken ct)
        {
            return Task.Run(() => DXResult<DXRelationItemUnit>.OkSkipProcess(dxUnit));
        }
    }
}