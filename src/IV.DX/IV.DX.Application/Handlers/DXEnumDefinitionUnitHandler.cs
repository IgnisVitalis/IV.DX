using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.HandlerContext;
using IV.DX.Application.Contracts.Handlers;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Application.Handlers
{
    internal class DXEnumDefinitionUnitHandler(IDXUnitDataService dxUnitService, IDXStructureRepository dataStructureRepo, IDXGenericRepository genericRepo) :
        DXObjectDefinitionUnitHandler(dxUnitService, dataStructureRepo, genericRepo),
        IDXBeforeInsert<DXEnumDefinitionUnit>,
        IDXBeforeUpdate<DXEnumDefinitionUnit>,
        IDXBeforeDelete<DXEnumDefinitionUnit>
    {
        public int BeforeOrder => throw new NotImplementedException();
              
        public Task<DXResult<DXEnumDefinitionUnit>> BeforeInsertAsync(DXEnumDefinitionUnit dxUnit, IDXHandlerContext ctx, CancellationToken ct)
        {
            base.Validate(dxUnit);
            base.Process(dxUnit);

            if (ctx is DXUnitHandlerPreInitCoreContextOld)
            {
                dataStructureRepo.CreateDataStructure(dxUnit);

                return Task.Run(() => DXResult<DXEnumDefinitionUnit>.OkSkipProcess(dxUnit));
            }
            else if (ctx is DXUnitHandlerPostInitCoreContextOld)
            {
                return Task.Run(() => DXResult<DXEnumDefinitionUnit>.OkContinue(dxUnit));
            }
            else
            {
                dataStructureRepo.CreateDataStructure(dxUnit);

                return Task.Run(() => DXResult<DXEnumDefinitionUnit>.OkContinue(dxUnit));
            }
        }

        public Task<DXResult<DXEnumDefinitionUnit>> BeforeUpdateAsync(DXEnumDefinitionUnit dxUnit, IDXHandlerContext ctx, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<DXResult> BeforeDeleteAsync(Guid id, IDXHandlerContext ctx, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
