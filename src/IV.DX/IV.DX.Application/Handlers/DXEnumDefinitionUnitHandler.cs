using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.HandlerContext;
using IV.DX.Application.Contracts.Handlers;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Enums;
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
        public int BeforeOrder => 1;

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
            base.Validate(dxUnit);
            base.Process(dxUnit);

            dataStructureRepo.UpdatedDataStructure(dxUnit);

            return Task.Run(() => DXResult<DXEnumDefinitionUnit>.OkContinue(dxUnit));
        }

        public Task<DXResult> BeforeDeleteAsync(Guid id, IDXHandlerContext ctx, CancellationToken ct)
        {
            var entity = genericRepo.GetItem<DXEnumDefinitionUnit>(id);

            base.Validate(entity);
            base.Process(entity);

            dataStructureRepo.DropDataStructure(entity);

            switch (entity.DXUnitDefinitionMainElement.Kind)
            {
                case DXObjectKindEnum.Core:
                    return Task.Run(() => DXResult.OkSkipProcess());
                default:
                    return Task.Run(() => DXResult.OkContinue());
            }
        }
    }
}
