using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.Handlers;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Application.Handlers
{
    internal class DXElementDefinitionUnitHandler(
        IDXUnitDataService dxUnitService,
        IDXStructureRepository dataStructureRepo,
        IDXUnitGenericRepository genericRepo,
        IDXElementGenericRepository dxElementGenericRepo,
        IDXStructureCache dxStructureCache) :
        DXObjectDefinitionUnitHandler(dxUnitService, dataStructureRepo, genericRepo, dxElementGenericRepo),
        IDXBeforeInsertHandler<DXElementDefinitionUnit>, IDXUniqueBeforeInsertHandler,
        IDXBeforeUpdateHandler<DXElementDefinitionUnit>, IDXUniqueBeforeUpdateHandler,
        IDXBeforeDeleteHandler<DXElementDefinitionUnit>, IDXUniqueBeforeDeleteHandler,
        IDXAfterInsertHandler<DXElementDefinitionUnit>, IDXUniqueAfterInsertHandler,
        IDXAfterUpdateHandler<DXElementDefinitionUnit>, IDXUniqueAfterUpdateHandler,
        IDXAfterDeleteHandler<DXElementDefinitionUnit>, IDXUniqueAfterDeleteHandler
    {
        public int BeforeOrder => 1;

        public int AfterOrder => 1;

        public async Task<DXResult<DXElementDefinitionUnit>> BeforeInsertAsync(DXElementDefinitionUnit dxUnit, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            base.Validate(dxUnit);
            base.Process(dxUnit, ctx);

            if (ctx is DXUnitHandlerPreInitCoreContext)
            {
                dataStructureRepo.CreateDataStructure(dxUnit);
                dataStructureRepo.UpdateUniqueColumns(dxUnit);

                return DXResult<DXElementDefinitionUnit>.OkSkipProcess(dxUnit);
            }
            else if (ctx is DXUnitHandlerPostInitCoreContext)
            {
                return DXResult<DXElementDefinitionUnit>.OkContinue(dxUnit);
            }
            else
            {
                dataStructureRepo.CreateDataStructure(dxUnit);

                await this.ProcessRelationsAsync(dxUnit, null, ct);

                dataStructureRepo.UpdateUniqueColumns(dxUnit);

                return DXResult<DXElementDefinitionUnit>.OkContinue(dxUnit);
            }
        }

        public async Task<DXResult<DXElementDefinitionUnit>> BeforeUpdateAsync(DXElementDefinitionUnit dxUnit, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            base.Validate(dxUnit);
            base.Process(dxUnit, ctx);

            dataStructureRepo.UpdatedDataStructure(dxUnit);

            var existingDXUnit = genericRepo.GetDXUnit<DXElementDefinitionUnit>(dxUnit.ID);

            await this.ProcessRelationsAsync(dxUnit, existingDXUnit, ct);

            return DXResult<DXElementDefinitionUnit>.OkContinue(dxUnit);
        }

        public async Task<DXResult<DXElementDefinitionUnit>> BeforeDeleteAsync(DXElementDefinitionUnit dxUnit, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            base.Validate(dxUnit);
            base.Process(dxUnit, ctx);

            dataStructureRepo.DropDataStructure(dxUnit);

            return DXResult<DXElementDefinitionUnit>.OkContinue(dxUnit);
        }

        private async Task ProcessRelationsAsync(DXElementDefinitionUnit dxUnit, DXElementDefinitionUnit? dxUnitExisting, CancellationToken ct)
        {
            await this.ProcessEnumRelationsAsync(dxUnit, dxUnitExisting, ct);
        }

        public async Task<DXResult> AfterInsertAsync(DXElementDefinitionUnit dxUnit, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            await dxStructureCache.RefreshAsync(ct);

            return DXResult.Ok();
        }

        public async Task<DXResult> AfterUpdateAsync(DXElementDefinitionUnit dxUnit, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            await dxStructureCache.RefreshAsync(ct);

            return DXResult.Ok();
        }

        public async Task<DXResult> AfterDeleteAsync(DXElementDefinitionUnit dxUnit, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            await dxStructureCache.RefreshAsync(ct);

            return DXResult.Ok();
        }
    }
}
