using IV.DX.Application.Contracts.Handlers;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Application.Handlers
{
    internal class DXDerivedUnitHandler(IDXStructureRepository dataStructureRepo) :
        IDXIsItemExistingHandler<DXDerivedUnit>, IDXUniqueIsItemExistingHandler,
        IDXBeforeInsertHandler<DXDerivedUnit>, IDXUniqueBeforeInsertHandler
    {
        public int BeforeOrder => 1;

        public Task<DXResult<bool>> IsItemExistingAsync(Guid id, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            return Task.FromResult(DXResult<bool>.OkSkipProcess(false));
        }

        public Task<DXResult<DXDerivedUnit>> BeforeInsertAsync(DXDerivedUnit dxUnit, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            var elements = dxUnit.DXDerivedElement?.Announced;
            if (elements != null)
            {
                foreach (var element in elements)
                {
                    dataStructureRepo.UpdateColumnValue(
                        nameof(DXObjectDefinitionUnit),
                        Constants.DerivedDXUnitType,
                        element.DerivedDXUnitType,
                        element.DXObjectId);
                }

                dataStructureRepo.SetColumnNotNull(nameof(DXObjectDefinitionUnit), Constants.DerivedDXUnitType);

                dataStructureRepo.UpdateColumnValue(
                    nameof(DXRelationDefinitionUnit),
                    "RelationType",
                    (int)DXRelationTypeEnum.ManyToOne,
                    new Dictionary<string, object>
                    {
                        ["ObjectNameLeft"] = nameof(DXObjectDefinitionUnit),
                        ["RelationNameLeft"] = "DXObjectDefinitionUnitItems",
                        ["ObjectNameRight"] = nameof(DXUnitDefinitionUnit),
                        ["RelationNameRight"] = Constants.DerivedDXUnitType
                    });

                dataStructureRepo.UpdateColumnValue(
                    nameof(DXRelationDefinitionUnit),
                    "RelationType",
                    (int)DXRelationTypeEnum.OneToMany,
                    new Dictionary<string, object>
                    {
                        ["ObjectNameLeft"] = nameof(DXUnitDefinitionUnit),
                        ["RelationNameLeft"] = Constants.DerivedDXUnitType,
                        ["ObjectNameRight"] = nameof(DXObjectDefinitionUnit),
                        ["RelationNameRight"] = "DXObjectDefinitionUnitItems"
                    });
            }

            return Task.FromResult(DXResult<DXDerivedUnit>.OkSkipProcess(dxUnit));
        }
    }
}
