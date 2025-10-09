using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.HandlerContext;
using IV.DX.Application.Contracts.Handlers;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Application.Handlers
{
    internal class DXUnitDefinitionUnitHandler(IDXUnitDataService dxUnitService, IDXGenericRepository genericRepo, IDXStructureRepository dataStructureRepo) :
        DXObjectDefinitionUnitHandler(dxUnitService, dataStructureRepo, genericRepo),
        IDXBeforeInsert<DXUnitDefinitionUnit>,
        IDXBeforeUpdate<DXUnitDefinitionUnit>,
        IDXBeforeDelete<DXUnitDefinitionUnit>
    {
        public int BeforeOrder => 1;

        public Task<DXResult<DXUnitDefinitionUnit>> BeforeInsertAsync(DXUnitDefinitionUnit dxUnit, IDXHandlerContext ctx, CancellationToken ct)
        {
            base.Validate(entity);
            base.Process(entity);

            if (context is DXUnitHandlerPreInitCoreContextOld)
            {
                this._dataStructureRepo.CreateDataStructure(entity);

                return Guid.Empty;
            }
            else if (context is DXUnitHandlerPostInitCoreContextOld)
            {
                return base.OnInserting(entity, context);
            }
            else
            {
                this._dataStructureRepo.CreateDataStructure(entity);

                this.ProcessRelations(entity);
                return base.OnInserting(entity, context);
            }
        }

        public Task<DXResult<DXUnitDefinitionUnit>> BeforeUpdateAsync(DXUnitDefinitionUnit dxUnit, IDXHandlerContext ctx, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<DXResult> BeforeDeleteAsync(Guid id, IDXHandlerContext ctx, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
