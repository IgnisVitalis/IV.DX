using IV.DX.Kernel.Converters;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using IV.DX.Persistence.CoreData;

namespace IV.DX.Persistence
{
    internal partial class DXCoreRepository : IDXCoreRepository, IDXStructureRepository, IDXEnumCoreRepository, IDXStructureRawReader
    {
        public IEnumerable<DXElementDefinitionUnit> LoadBlockInfosRaw()
        {
            if (DXMaintenanceToken.IsCoreInitializing)
                return DXElementDefinitionUnitItems.Items;

            var blockModelsFromDB = this.GetItems(DXElementDefinitionUnit.ESQLModelDefinition, DXLoadingType.Full);

            var blockInfos = blockModelsFromDB.Select(x => DXUnitHelper.CreateInstance<DXElementDefinitionUnit>(x));

            var blockInfosWithoutCore = blockInfos.Except(DXCoreDataStructureRepository.CoreBlockInfos, DXObjectDefinitionUnitIDComparer.Instance)
                .Select(x => x as DXElementDefinitionUnit);

            return DXCoreDataStructureRepository.CoreBlockInfos.Concat(blockInfosWithoutCore).ToList();
        }

        public IEnumerable<DXUnitDefinitionUnit> LoadEntityInfosRaw()
        {
            if (DXMaintenanceToken.IsCoreInitializing)
                return DXUnitDefinitionUnitItems.Items;

            var dxUnitModelsFromDB = this.GetItems(DXUnitDefinitionUnit.ESQLModelDefinition, DXLoadingType.Full);

            var dxUnitInfos = dxUnitModelsFromDB.Select(x => DXUnitHelper.CreateInstance<DXUnitDefinitionUnit>(x));

            var dxUnitInfosWithoutCore = dxUnitInfos.Except(DXCoreDataStructureRepository.CoreEntityInfos, DXObjectDefinitionUnitIDComparer.Instance)
                .Select(x => x as DXUnitDefinitionUnit);

            return DXCoreDataStructureRepository.CoreEntityInfos.Concat(dxUnitInfosWithoutCore).ToList();
        }

        public IEnumerable<DXEnumDefinitionUnit> LoadEnumInfosRaw()
        {
            if (DXMaintenanceToken.IsCoreInitializing)
                return DXEnumDefinitionUnitItems.Items;

            var enumsModelsFromDB = this.GetItems(DXEnumDefinitionUnit.ESQLModelDefinition, DXLoadingType.Full);

            var enumInfos = enumsModelsFromDB.Select(x => DXUnitHelper.CreateInstance<DXEnumDefinitionUnit>(x));

            var enumInfosWithoutCore = enumInfos.Except(DXCoreDataStructureRepository.CoreEnumInfos, DXObjectDefinitionUnitIDComparer.Instance)
                .Select(x => x as DXEnumDefinitionUnit);

            return DXCoreDataStructureRepository.CoreEnumInfos.Concat(enumInfosWithoutCore).ToList();
        }

        public IEnumerable<DXRelationDefinitionUnit> LoadRelationInfosRaw()
        {
            if (DXMaintenanceToken.IsCoreInitializing)
                return Enumerable.Empty<DXRelationDefinitionUnit>();

            var result = this.GetItems(DXRelationDefinitionUnit.ESQLModelDefinition, DXLoadingType.Full);
            return result.Select(x => DXUnitHelper.CreateInstance<DXRelationDefinitionUnit>(x)).ToList();
        }
    }
}