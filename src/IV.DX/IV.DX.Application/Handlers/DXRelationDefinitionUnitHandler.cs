using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.Handlers;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Application.Handlers
{
    internal class DXRelationDefinitionUnitHandler(IDXUnitDataService dxUnitService, IDXUnitGenericRepository genericRepo, IDXStructureRepository dataStructureRepo) :
        IDXBeforeInsertHandler<DXRelationDefinitionUnit>, IDXUniqueBeforeInsertHandler,
        IDXAfterInsertHandler<DXRelationDefinitionUnit>, IDXUniqueAfterInsertHandler,
        IDXBeforeUpdateHandler<DXRelationDefinitionUnit>, IDXUniqueBeforeUpdateHandler,
        IDXBeforeDeleteHandler<DXRelationDefinitionUnit>, IDXUniqueBeforeDeleteHandler
    {
        public int BeforeOrder => 1;

        public int AfterOrder => throw new NotImplementedException();

        public Task<DXResult<DXRelationDefinitionUnit>> BeforeInsertAsync(DXRelationDefinitionUnit dxUnit, IDXHandlerContext ctx, CancellationToken ct)
        {
            var existingRelation = dataStructureRepo.GetDXRelationDefinition(dxUnit.DXRelationDefinitionMainElement.ObjectNameLeft, dxUnit.DXRelationDefinitionMainElement.RelationNameLeft, dxUnit.DXRelationDefinitionMainElement.ObjectNameRight, dxUnit.DXRelationDefinitionMainElement.RelationNameRight);

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
                return Task.Run(() => DXResult<DXRelationDefinitionUnit>.OkContinue(dxUnit));
            }
            else
            {
                dataStructureRepo.CreateDataStructure(dxUnit);

                return Task.Run(() => DXResult<DXRelationDefinitionUnit>.OkContinue(dxUnit));
            }
        }

        public Task<DXResult> AfterInsertAsync(DXRelationDefinitionUnit dxUnit, IDXHandlerContext ctx, CancellationToken ct)
        {
            if (ctx is DXUnitHandlerPreInitCoreContext)
            {
                return Task.Run(() => DXResult.OkSkipProcess());
            }
            else
            {
                var invertedRelation = dxUnit.CreateInvertedRelationObject();

                genericRepo.Insert(invertedRelation);

                return Task.Run(() => DXResult.OkContinue());
            }
        }

        public Task<DXResult<DXRelationDefinitionUnit>> BeforeUpdateAsync(DXRelationDefinitionUnit dxUnit, IDXHandlerContext ctx, CancellationToken ct)
        {
            return Task.Run(() => DXResult<DXRelationDefinitionUnit>.OkSkipProcess(dxUnit));
        }

        public Task<DXResult<DXRelationDefinitionUnit>> BeforeDeleteAsync(DXRelationDefinitionUnit dxUnit, IDXHandlerContext ctx, CancellationToken ct)
        {
            //if (ctx is DXRelationDefinitionUnitInvertedItemContext)
            //{
            //    return Task.Run(() => DXResult<DXRelationDefinitionUnit>.OkContinue(dxUnit));
            //}

            dataStructureRepo.DropDataStructure(dxUnit);

            var existingRelation = dataStructureRepo.GetDXRelationDefinition(dxUnit.DXRelationDefinitionMainElement.ObjectNameLeft, dxUnit.DXRelationDefinitionMainElement.RelationNameLeft, dxUnit.DXRelationDefinitionMainElement.ObjectNameRight, dxUnit.DXRelationDefinitionMainElement.RelationNameRight);

            if (existingRelation == null)
                return Task.Run(() => DXResult<DXRelationDefinitionUnit>.OkSkipProcess(dxUnit));

            dxUnit = existingRelation;
            var invertedRelation = this.GetInvertedRelationObject(dxUnit);

            genericRepo.Delete(invertedRelation);

            return Task.Run(() => DXResult<DXRelationDefinitionUnit>.OkContinue(dxUnit));
        }

        private DXRelationDefinitionUnit GetInvertedRelationObject(DXRelationDefinitionUnit dxUnit)
        {
            var modelDefinition = genericRepo.GetDXUnits<DXRelationDefinitionUnit>(dxUnit.GetQueryForInvertedRelationObject());

            return modelDefinition.SingleOrDefault();
        }

        //private class DXRelationDefinitionUnitInvertedItemContext : IDXHandlerContext
        //{

        //}
    }
}
