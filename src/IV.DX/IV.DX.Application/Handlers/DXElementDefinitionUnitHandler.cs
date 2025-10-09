using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.Handlers;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Application.Handlers
{
    internal class DXElementDefinitionUnitHandler(IDXUnitDataService dxUnitService, IDXStructureRepository dataStructureRepo, IDXGenericRepository genericRepo) :
        DXObjectDefinitionUnitHandler(dxUnitService, dataStructureRepo, genericRepo),
        IDXBeforeInsert<DXElementDefinitionUnit>,
        IDXBeforeUpdate<DXElementDefinitionUnit>,
        IDXBeforeDelete<DXElementDefinitionUnit>
    {
        public int BeforeOrder => 1;

        public Task<DXResult<DXElementDefinitionUnit>> BeforeInsertAsync(DXElementDefinitionUnit dxUnit, IDXHandlerContext ctx, CancellationToken ct)
        {
            base.Validate(dxUnit);
            base.Process(dxUnit);

            if (ctx is DXUnitHandlerPreInitCoreContext)
            {
                dataStructureRepo.CreateDataStructure(dxUnit);

                return Task.Run(() => DXResult<DXElementDefinitionUnit>.OkSkipProcess(dxUnit));
            }
            else if (ctx is DXUnitHandlerPostInitCoreContext)
            {
                return Task.Run(() => DXResult<DXElementDefinitionUnit>.OkContinue(dxUnit));
            }
            else
            {
                dataStructureRepo.CreateDataStructure(dxUnit);

                this.ProcessRelations(dxUnit);

                return Task.Run(() => DXResult<DXElementDefinitionUnit>.OkContinue(dxUnit));
            }
        }

        public Task<DXResult<DXElementDefinitionUnit>> BeforeUpdateAsync(DXElementDefinitionUnit dxUnit, IDXHandlerContext ctx, CancellationToken ct)
        {
            base.Validate(dxUnit);
            base.Process(dxUnit);

            dataStructureRepo.UpdatedDataStructure(dxUnit);

            this.ProcessRelations(dxUnit);

            return Task.Run(() => DXResult<DXElementDefinitionUnit>.OkContinue(dxUnit));
        }

        public Task<DXResult> BeforeDeleteAsync(Guid id, IDXHandlerContext ctx, CancellationToken ct)
        {
            var entity = genericRepo.GetItem<DXElementDefinitionUnit>(id);

            if (entity == null)
                return Task.Run(() => DXResult.Ok(DXFlow.SkipProcess));

            base.Validate(entity);
            base.Process(entity);

            dataStructureRepo.DropDataStructure(entity);

            return Task.Run(() => DXResult.OkContinue());
        }

        private void ProcessRelations(DXElementDefinitionUnit entity)
        {
            this.ProcessEnumRelations(entity);
        }
    }
}
