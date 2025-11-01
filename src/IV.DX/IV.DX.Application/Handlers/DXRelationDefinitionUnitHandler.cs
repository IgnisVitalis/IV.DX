using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.Handlers;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Application.Handlers
{
    internal class DXRelationDefinitionUnitHandler(
        IDXUnitDataService dxUnitService,
        IDXUnitGenericRepository genericRepo,
        IDXStructureRepository dataStructureRepo,
        IDXStructureCache dxStructureCache) :
        IDXBeforeInsertHandler<DXRelationDefinitionUnit>, IDXUniqueBeforeInsertHandler,
        IDXBeforeUpdateHandler<DXRelationDefinitionUnit>, IDXUniqueBeforeUpdateHandler,
        IDXBeforeDeleteHandler<DXRelationDefinitionUnit>, IDXUniqueBeforeDeleteHandler,
        IDXAfterInsertHandler<DXRelationDefinitionUnit>, IDXUniqueAfterInsertHandler,
        IDXAfterUpdateHandler<DXRelationDefinitionUnit>, IDXUniqueAfterUpdateHandler,
        IDXAfterDeleteHandler<DXRelationDefinitionUnit>, IDXUniqueAfterDeleteHandler
    {
        public int BeforeOrder => 1;

        public int AfterOrder => 1;

        public async Task<DXResult<DXRelationDefinitionUnit>> BeforeInsertAsync(DXRelationDefinitionUnit dxUnit, IDXHandlerContext ctx, CancellationToken ct)
        {
            var existingRelation = dataStructureRepo.GetDXRelationDefinition(dxUnit.DXRelationDefinitionMainElement.ObjectNameLeft, dxUnit.DXRelationDefinitionMainElement.RelationNameLeft, dxUnit.DXRelationDefinitionMainElement.ObjectNameRight, dxUnit.DXRelationDefinitionMainElement.RelationNameRight);

            if (existingRelation != null)
            {
                return DXResult<DXRelationDefinitionUnit>.OkSkipProcess(dxUnit);
            }

            if (ctx is DXUnitHandlerPreInitCoreContext)
            {
                dataStructureRepo.CreateDataStructure(dxUnit);

                return DXResult<DXRelationDefinitionUnit>.OkSkipProcess(dxUnit);
            }
            else if (ctx is DXUnitHandlerPostInitCoreContext)
            {
                return DXResult<DXRelationDefinitionUnit>.OkContinue(dxUnit);
            }
            else
            {
                dataStructureRepo.CreateDataStructure(dxUnit);

                return DXResult<DXRelationDefinitionUnit>.OkContinue(dxUnit);
            }
        }

        public async Task<DXResult> AfterInsertAsync(DXRelationDefinitionUnit dxUnit, IDXHandlerContext ctx, CancellationToken ct)
        {
            if (ctx is DXUnitHandlerPreInitCoreContext)
            {
                return DXResult.OkSkipProcess();
            }
            else
            {
                var invertedRelation = dxUnit.CreateInvertedRelationObject();

                genericRepo.Insert(invertedRelation);

                await dxStructureCache.RefreshAsync(ct);               

                return DXResult.OkContinue();
            }
        }

        public async Task<DXResult<DXRelationDefinitionUnit>> BeforeUpdateAsync(DXRelationDefinitionUnit dxUnit, IDXHandlerContext ctx, CancellationToken ct)
        {
            return DXResult<DXRelationDefinitionUnit>.OkSkipProcess(dxUnit);
        }

        public async Task<DXResult<DXRelationDefinitionUnit>> BeforeDeleteAsync(DXRelationDefinitionUnit dxUnit, IDXHandlerContext ctx, CancellationToken ct)
        {
            //if (ctx is DXRelationDefinitionUnitInvertedItemContext)
            //{
            //    return Task.Run(() => DXResult<DXRelationDefinitionUnit>.OkContinue(dxUnit));
            //}

            dataStructureRepo.DropDataStructure(dxUnit);

            var existingRelation = dataStructureRepo.GetDXRelationDefinition(dxUnit.DXRelationDefinitionMainElement.ObjectNameLeft, dxUnit.DXRelationDefinitionMainElement.RelationNameLeft, dxUnit.DXRelationDefinitionMainElement.ObjectNameRight, dxUnit.DXRelationDefinitionMainElement.RelationNameRight);

            if (existingRelation == null)
                return DXResult<DXRelationDefinitionUnit>.OkSkipProcess(dxUnit);

            dxUnit = existingRelation;
            var invertedRelation = this.GetInvertedRelationObject(dxUnit);

            genericRepo.Delete(invertedRelation);

            return DXResult<DXRelationDefinitionUnit>.OkContinue(dxUnit);
        }

        private DXRelationDefinitionUnit GetInvertedRelationObject(DXRelationDefinitionUnit dxUnit)
        {
            var modelDefinition = genericRepo.GetDXUnits<DXRelationDefinitionUnit>(dxUnit.GetQueryForInvertedRelationObject());

            return modelDefinition.SingleOrDefault();
        }

        public async Task<DXResult> AfterDeleteAsync(DXRelationDefinitionUnit dxUnit, IDXHandlerContext ctx, CancellationToken ct)
        {
            await dxStructureCache.RefreshAsync(ct);

            return DXResult.Ok();
        }

        public async Task<DXResult> AfterUpdateAsync(DXRelationDefinitionUnit dxUnit, IDXHandlerContext ctx, CancellationToken ct)
        {
            await dxStructureCache.RefreshAsync(ct);

            return DXResult.Ok();
        }
    }
}
