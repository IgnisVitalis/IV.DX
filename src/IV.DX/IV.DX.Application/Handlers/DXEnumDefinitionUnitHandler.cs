using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.Handlers;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Application.Handlers
{
    internal class DXEnumDefinitionUnitHandler(
        IDXUnitDataService dxUnitService, 
        IDXStructureRepository dataStructureRepo, 
        IDXUnitGenericRepository genericRepo,
        IDXElementGenericRepository dxElementGenericRepo,
        IDXStructureCache dxStructureCache) :
        DXObjectDefinitionUnitHandler(dxUnitService, dataStructureRepo, genericRepo, dxElementGenericRepo),
        IDXBeforeInsertHandler<DXEnumDefinitionUnit>, IDXUniqueBeforeInsertHandler,
        IDXBeforeUpdateHandler<DXEnumDefinitionUnit>, IDXUniqueBeforeUpdateHandler,
        IDXBeforeDeleteHandler<DXEnumDefinitionUnit>, IDXUniqueBeforeDeleteHandler,
        IDXAfterInsertHandler<DXEnumDefinitionUnit>, IDXUniqueAfterInsertHandler,
        IDXAfterUpdateHandler<DXEnumDefinitionUnit>, IDXUniqueAfterUpdateHandler,
        IDXAfterDeleteHandler<DXEnumDefinitionUnit>, IDXUniqueAfterDeleteHandler
    {
        public int BeforeOrder => 1;

        public int AfterOrder => 1;

        public async Task<DXResult<DXEnumDefinitionUnit>> BeforeInsertAsync(DXEnumDefinitionUnit dxUnit, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            base.Validate(dxUnit);
            base.Process(dxUnit, ctx);

            if (ctx is DXUnitHandlerPreInitCoreContext)
            {
                dataStructureRepo.CreateDataStructure(dxUnit);
                dataStructureRepo.UpdateUniqueColumns(dxUnit);

                return DXResult<DXEnumDefinitionUnit>.OkSkipProcess(dxUnit);
            }
            else if (ctx is DXUnitHandlerPostInitCoreContext)
            {
                return DXResult<DXEnumDefinitionUnit>.OkContinue(dxUnit);
            }
            else
            {
                dataStructureRepo.CreateDataStructure(dxUnit);
                dataStructureRepo.UpdateUniqueColumns(dxUnit);

                return DXResult<DXEnumDefinitionUnit>.OkContinue(dxUnit);
            }
        }

        public async Task<DXResult<DXEnumDefinitionUnit>> BeforeUpdateAsync(DXEnumDefinitionUnit dxUnit, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            base.Validate(dxUnit);
            base.Process(dxUnit, ctx);

            dataStructureRepo.UpdatedDataStructure(dxUnit);

            return DXResult<DXEnumDefinitionUnit>.OkContinue(dxUnit);
        }

        public async Task<DXResult<DXEnumDefinitionUnit>> BeforeDeleteAsync(DXEnumDefinitionUnit dxUnit, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            base.Validate(dxUnit);
            base.Process(dxUnit, ctx);

            dataStructureRepo.DropDataStructure(dxUnit);

            switch (dxUnit.Kind)
            {
                case DXObjectKindEnum.Core:
                    return DXResult<DXEnumDefinitionUnit>.OkSkipProcess(dxUnit);
                default:
                    return DXResult<DXEnumDefinitionUnit>.OkContinue(dxUnit);
            }
        }

        public async Task<DXResult> AfterInsertAsync(DXEnumDefinitionUnit dxUnit, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            await dxStructureCache.RefreshAsync(ct);

            return DXResult.Ok();
        }

        public async Task<DXResult> AfterUpdateAsync(DXEnumDefinitionUnit dxUnit, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            await dxStructureCache.RefreshAsync(ct);

            return DXResult.Ok();
        }

        public async Task<DXResult> AfterDeleteAsync(DXEnumDefinitionUnit dxUnit, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            await dxStructureCache.RefreshAsync(ct);

            return DXResult.Ok();
        }       
    }
}