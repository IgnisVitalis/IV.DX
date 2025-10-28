using IV.DX.Kernel.Converters.DXModelDefinitionConverters;
using IV.DX.Kernel.Converters.DXObjectConverters;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using IV.DX.Persistence.CoreData;

namespace IV.DX.Persistence
{
    internal partial class DXCoreRepository : IDXCoreRepository, IDXStructureRepository, IDXEnumCoreRepository, IDXStructureRawReader
    {
        public IEnumerable<DXElementDefinitionUnit> LoadDXElementInfosRaw()
        {
            if (DXMaintenanceToken.IsCoreInitializing)
                return DXElementDefinitionUnitItems.Items;

            var dxElementModelsFromDB = this.GetItems(DXModelDefinitionConverter.Get<DXElementDefinitionUnit>(), DXLoadingType.Full);

            var dxElementInfos = dxElementModelsFromDB.Select(x => DXUnitConverter.Parse<DXElementDefinitionUnit>(x));

            var dxElementInfosWithoutCore = dxElementInfos.Except(DXCoreDataStructureRepository.CoreDXElementInfos, DXObjectDefinitionUnitIDComparer.Instance)
                .Select(x => x as DXElementDefinitionUnit);

            return DXCoreDataStructureRepository.CoreDXElementInfos.Concat(dxElementInfosWithoutCore).ToList();
        }

        public IEnumerable<DXUnitDefinitionUnit> LoadDXUnitInfosRaw()
        {
            if (DXMaintenanceToken.IsCoreInitializing)
                return DXUnitDefinitionUnitItems.Items;

            var dxUnitModelsFromDB = this.GetItems(DXModelDefinitionConverter.Get<DXUnitDefinitionUnit>(), DXLoadingType.Full);

            var dxUnitInfos = dxUnitModelsFromDB.Select(x => DXUnitConverter.Parse<DXUnitDefinitionUnit>(x));

            var dxUnitInfosWithoutCore = dxUnitInfos.Except(DXCoreDataStructureRepository.CoreDXUnitInfos, DXObjectDefinitionUnitIDComparer.Instance)
                .Select(x => x as DXUnitDefinitionUnit);

            return DXCoreDataStructureRepository.CoreDXUnitInfos.Concat(dxUnitInfosWithoutCore).ToList();
        }

        public IEnumerable<DXEnumDefinitionUnit> LoadDXEnumInfosRaw()
        {
            if (DXMaintenanceToken.IsCoreInitializing)
                return DXEnumDefinitionUnitItems.Items;

            var enumsModelsFromDB = this.GetItems(DXModelDefinitionConverter.Get<DXEnumDefinitionUnit>(), DXLoadingType.Full);

            var enumInfos = enumsModelsFromDB.Select(x => DXUnitConverter.Parse<DXEnumDefinitionUnit>(x));

            var enumInfosWithoutCore = enumInfos.Except(DXCoreDataStructureRepository.CoreEnumInfos, DXObjectDefinitionUnitIDComparer.Instance)
                .Select(x => x as DXEnumDefinitionUnit);

            return DXCoreDataStructureRepository.CoreEnumInfos.Concat(enumInfosWithoutCore).ToList();
        }

        public IEnumerable<DXRelationDefinitionUnit> LoadDXRelationInfosRaw()
        {
            if (DXMaintenanceToken.IsCoreInitializing)
                return Enumerable.Empty<DXRelationDefinitionUnit>();

            var result = this.GetItems(DXModelDefinitionConverter.Get<DXRelationDefinitionUnit>(), DXLoadingType.Full);
            return result.Select(x => DXUnitConverter.Parse<DXRelationDefinitionUnit>(x)).ToList();
        }
    }
}