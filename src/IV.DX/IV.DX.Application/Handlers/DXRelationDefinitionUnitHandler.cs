using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.Handlers;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Application.Handlers
{
    internal class DXRelationDefinitionUnitHandler(
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

        public Task<DXResult<DXRelationDefinitionUnit>> BeforeInsertAsync(DXRelationDefinitionUnit dxUnit, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            if (ctx is DXUnitHandlerPreInitCoreContext)
            {
                dataStructureRepo.CreateDataStructure(dxUnit);

                return Task.FromResult(DXResult<DXRelationDefinitionUnit>.OkSkipProcess(dxUnit));
            }
            else if (ctx is DXUnitHandlerPostInitCoreContext)
            {
                return Task.FromResult(DXResult<DXRelationDefinitionUnit>.OkContinue(dxUnit));
            }
            else
            {
                var existingRelation = dataStructureRepo.GetDXRelationDefinition(dxUnit.ObjectNameLeft, dxUnit.RelationNameLeft, dxUnit.ObjectNameRight, dxUnit.RelationNameRight);

                if (existingRelation != null)
                {
                    return Task.FromResult(DXResult<DXRelationDefinitionUnit>.OkSkipProcess(dxUnit));
                }
                else if (ctx is DXUnitHandlerEnumProcessingContext)
                {
                    dataStructureRepo.CreateDataStructure(dxUnit);

                    return Task.FromResult(DXResult<DXRelationDefinitionUnit>.OkContinue(dxUnit));
                }
                else
                {
                    dataStructureRepo.CreateDataStructure(dxUnit);

                    return Task.FromResult(DXResult<DXRelationDefinitionUnit>.OkContinue(dxUnit));
                }
            }
        }

        public async Task<DXResult> AfterInsertAsync(DXRelationDefinitionUnit dxUnit, DXHandlerBaseContext ctx, CancellationToken ct)
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

        public Task<DXResult<DXRelationDefinitionUnit>> BeforeUpdateAsync(DXRelationDefinitionUnit dxUnit, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            throw new NotImplementedException("The update method for DXRelationDefinitionUnit isn't implemented yet");
        }

        public Task<DXResult<DXRelationDefinitionUnit>> BeforeDeleteAsync(DXRelationDefinitionUnit dxUnit, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            dataStructureRepo.DropDataStructure(dxUnit);

            var existingRelation = dataStructureRepo.GetDXRelationDefinition(dxUnit.ObjectNameLeft, dxUnit.RelationNameLeft, dxUnit.ObjectNameRight, dxUnit.RelationNameRight);

            if (existingRelation == null)
                return Task.FromResult(DXResult<DXRelationDefinitionUnit>.OkSkipProcess(dxUnit));

            dxUnit = existingRelation;
            var invertedRelation = this.GetInvertedRelationObject(dxUnit);

            genericRepo.Delete(invertedRelation!);

            return Task.FromResult(DXResult<DXRelationDefinitionUnit>.OkContinue(dxUnit));
        }

        private DXRelationDefinitionUnit? GetInvertedRelationObject(DXRelationDefinitionUnit dxUnit)
        {
            var modelDefinition = genericRepo.GetDXUnits<DXRelationDefinitionUnit>(dxUnit.GetQueryForInvertedRelationObject());

            return modelDefinition.SingleOrDefault();
        }

        public async Task<DXResult> AfterDeleteAsync(DXRelationDefinitionUnit dxUnit, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            await dxStructureCache.RefreshAsync(ct);

            return DXResult.Ok();
        }

        public async Task<DXResult> AfterUpdateAsync(DXRelationDefinitionUnit dxUnit, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            await dxStructureCache.RefreshAsync(ct);

            return DXResult.Ok();
        }
    }
}