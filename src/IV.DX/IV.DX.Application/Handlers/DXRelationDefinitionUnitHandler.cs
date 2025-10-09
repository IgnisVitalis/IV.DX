using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.Handlers;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Application.Handlers
{
    internal class DXRelationDefinitionUnitHandler(IDXUnitDataService dxUnitService, IDXGenericRepository genericRepo, IDXStructureRepository dataStructureRepo) :
        IDXBeforeInsert<DXRelationDefinitionUnit>,
        IDXBeforeUpdate<DXRelationDefinitionUnit>,
        IDXBeforeDelete<DXRelationDefinitionUnit>
    {
        public int BeforeOrder => 1;

        public Task<DXResult<DXRelationDefinitionUnit>> BeforeInsertAsync(DXRelationDefinitionUnit dxUnit, IDXHandlerContext ctx, CancellationToken ct)
        {
            var existingRelation = dataStructureRepo.GetRelation(dxUnit.DXRelationDefinitionMainElement.ObjectNameLeft, dxUnit.DXRelationDefinitionMainElement.RelationNameLeft, dxUnit.DXRelationDefinitionMainElement.ObjectNameRight, dxUnit.DXRelationDefinitionMainElement.RelationNameRight);

            if (existingRelation != null)
            {
                return Task.Run(() => DXResult<DXRelationDefinitionUnit>.OkSkipProcess(dxUnit));
            }

            if (ctx is DXUnitHandlerPreInitCoreContext)
            {
                dataStructureRepo.CreateDataStructure(dxUnit);

                return Task.Run(() => DXResult<DXRelationDefinitionUnit>.OkSkipProcess(dxUnit));
            }
            else if (ctx is DXUnitHandlerPostInitCoreContext)
            {
                var invertedRelation = dxUnit.CreateInvertedRelationObject();

                return Task.Run(() => DXResult<DXRelationDefinitionUnit>.OkContinue(dxUnit));
            }
            else
            {
                dataStructureRepo.CreateDataStructure(dxUnit);

                var invertedRelation = dxUnit.CreateInvertedRelationObject();

                return Task.Run(() => DXResult<DXRelationDefinitionUnit>.OkContinue(dxUnit));
            }
        }

        public Task<DXResult<DXRelationDefinitionUnit>> BeforeUpdateAsync(DXRelationDefinitionUnit dxUnit, IDXHandlerContext ctx, CancellationToken ct)
        {
            return Task.Run(() => DXResult<DXRelationDefinitionUnit>.OkSkipProcess(dxUnit));
        }

        public Task<DXResult<DXRelationDefinitionUnit>> BeforeDeleteAsync(DXRelationDefinitionUnit dxUnit, IDXHandlerContext ctx, CancellationToken ct)
        {
            if (ctx is DXRelationDefinitionUnitInvertedItemContext)
            {
                return Task.Run(() => DXResult< DXRelationDefinitionUnit>.OkContinue(dxUnit));
            }

            dataStructureRepo.DropDataStructure(dxUnit);

            var existingRelation = dataStructureRepo.GetRelation(dxUnit.DXRelationDefinitionMainElement.ObjectNameLeft, dxUnit.DXRelationDefinitionMainElement.RelationNameLeft, dxUnit.DXRelationDefinitionMainElement.ObjectNameRight, dxUnit.DXRelationDefinitionMainElement.RelationNameRight);

            if (existingRelation == null)
                return Task.Run(() => DXResult<DXRelationDefinitionUnit>.OkSkipProcess(dxUnit));

            dxUnit = existingRelation;
            var invertedRelation = this.GetInvertedRelationObject(dxUnit);

            dxUnitService.Delete("DXRelationDefinitionUnit", invertedRelation.ID, new DXRelationDefinitionUnitInvertedItemContext());

            return Task.Run(() => DXResult<DXRelationDefinitionUnit>.OkContinue(dxUnit));
        }

        private DXRelationDefinitionUnit GetInvertedRelationObject(DXRelationDefinitionUnit entity)
        {
            var modelDefinition = genericRepo.GetItems<DXRelationDefinitionUnit>(entity.GetQueryForInvertedRelationObject());

            return modelDefinition.SingleOrDefault();
        }

        private class DXRelationDefinitionUnitInvertedItemContext : IDXHandlerContext
        {

        }
    }
}
