using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.Handlers;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Application.Handlers
{
    internal class DXEnumDefinitionUnitHandler :
        DXObjectDefinitionUnitHandler,
        IDXBeforeInsertHandler<DXEnumDefinitionUnit>, IDXUniqueBeforeInsertHandler,
        IDXBeforeUpdateHandler<DXEnumDefinitionUnit>, IDXUniqueBeforeUpdateHandler,
        IDXBeforeDeleteHandler<DXEnumDefinitionUnit>, IDXUniqueBeforeDeleteHandler,
        IDXAfterInsertHandler<DXEnumDefinitionUnit>, IDXUniqueAfterInsertHandler,
        IDXAfterUpdateHandler<DXEnumDefinitionUnit>, IDXUniqueAfterUpdateHandler,
        IDXAfterDeleteHandler<DXEnumDefinitionUnit>, IDXUniqueAfterDeleteHandler
    {
        private readonly IDXStructureRepository dataStructureRepo;
        private readonly IDXStructureCache dxStructureCache;

        public DXEnumDefinitionUnitHandler(
            IDXUnitDataService dxUnitService,
            IDXStructureRepository dataStructureRepo,
            IDXUnitGenericRepository genericRepo,
            IDXElementGenericRepository dxElementGenericRepo,
            IDXStructureCache dxStructureCache)
            : base(dxUnitService, dataStructureRepo, genericRepo)
        {
            this.dataStructureRepo = dataStructureRepo;
            this.dxStructureCache = dxStructureCache;
        }
        public int BeforeOrder => 1;

        public int AfterOrder => 1;

        public async Task<DXResult<DXEnumDefinitionUnit>> BeforeInsertAsync(DXEnumDefinitionUnit dxUnit, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            base.Validate(dxUnit);
            base.Process(dxUnit, ctx);

            if (ctx is DXUnitHandlerPreInitCoreContext)
            {
                dataStructureRepo.CreateDataStructure(dxUnit);
                base.NormalizeUniqueColumnsBeforeSave(dxUnit, null);
                
                await base.ProcessUniqueColumnsAsync(dxUnit, null, ct);
               
                return DXResult<DXEnumDefinitionUnit>.OkSkipProcess(dxUnit);
            }
            else if (ctx is DXUnitHandlerPostInitCoreContext)
            {
                return DXResult<DXEnumDefinitionUnit>.OkContinue(dxUnit);
            }
            else
            {
                dataStructureRepo.CreateDataStructure(dxUnit);

                await base.ProcessUniqueColumnsAsync(dxUnit, null, ct);

                return DXResult<DXEnumDefinitionUnit>.OkContinue(dxUnit);
            }
        }

        public Task<DXResult<DXEnumDefinitionUnit>> BeforeUpdateAsync(DXEnumDefinitionUnit dxUnit, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            base.Validate(dxUnit);
            base.Process(dxUnit, ctx);

            dataStructureRepo.UpdatedDataStructure(dxUnit);

            return Task.FromResult(DXResult<DXEnumDefinitionUnit>.OkContinue(dxUnit));
        }

        public Task<DXResult<DXEnumDefinitionUnit>> BeforeDeleteAsync(DXEnumDefinitionUnit dxUnit, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            base.Validate(dxUnit);
            base.Process(dxUnit, ctx);

            dataStructureRepo.DropDataStructure(dxUnit);

            return dxUnit.Kind switch
            {
                DXObjectKindEnum.Core => Task.FromResult(DXResult<DXEnumDefinitionUnit>.OkSkipProcess(dxUnit)),
                _ => Task.FromResult(DXResult<DXEnumDefinitionUnit>.OkContinue(dxUnit))
            };
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