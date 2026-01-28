using IV.DX.Kernel.Converters.DXModelDefinitionConverters;
using IV.DX.Kernel.Converters.DXObjectConverters;
using IV.DX.Kernel.Data.Models;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Persistence
{
    internal partial class DXCoreRepository : IDXUnitCoreRepository, IDXStructureRepository, IDXEnumCoreRepository, IDXStructureRawReader, IDXElementCoreRepository, IDXRawReader
    {
        public IEnumerable<DXElementDefinitionUnit> LoadDXElementInfosRaw()
        {
            if (DXMaintenanceToken.IsCoreInitializing)
                return DXElementDefinitionUnitItems.Items;

            var dxElementModelsFromDB = this.GetItems(DXDataSetDefinitionConverter.ToDXModelDefinition<DXElementDefinitionUnit>(), DXLoadingType.Full);

            var dxElementInfos = dxElementModelsFromDB.Select(x => DXUnitConverter.ToDXUnits<DXElementDefinitionUnit>(x)).ToList();

            return dxElementInfos;
        }

        public IEnumerable<DXUnitDefinitionUnit> LoadDXUnitInfosRaw()
        {
            if (DXMaintenanceToken.IsCoreInitializing)
                return DXUnitDefinitionUnitItems.Items;

            var dxUnitModelsFromDB = this.GetItems(DXDataSetDefinitionConverter.ToDXModelDefinition<DXUnitDefinitionUnit>(), DXLoadingType.Full);

            var dxUnitInfos = dxUnitModelsFromDB.Select(x => DXUnitConverter.ToDXUnits<DXUnitDefinitionUnit>(x)).ToList();

            return dxUnitInfos;
        }

        public IEnumerable<DXEnumDefinitionUnit> LoadDXEnumInfosRaw()
        {
            if (DXMaintenanceToken.IsCoreInitializing)
                return DXEnumDefinitionUnitItems.Items;

            var enumsModelsFromDB = this.GetItems(DXDataSetDefinitionConverter.ToDXModelDefinition<DXEnumDefinitionUnit>(), DXLoadingType.Full);

            var enumInfos = enumsModelsFromDB.Select(x => DXUnitConverter.ToDXUnits<DXEnumDefinitionUnit>(x)).ToList();

            return enumInfos;
        }

        public IEnumerable<DXRelationDefinitionUnit> LoadDXRelationInfosRaw()
        {
            if (DXMaintenanceToken.IsCoreInitializing)
                //return Enumerable.Empty<DXRelationDefinitionUnit>();
                return DXRelationDefinitionUnitItems.Items;
            
            
            
            // Enumerable.Empty<DXRelationDefinitionUnit>();

            var result = this.GetItems(DXDataSetDefinitionConverter.ToDXModelDefinition<DXRelationDefinitionUnit>(), DXLoadingType.Full);

            return result.Select(x => DXUnitConverter.ToDXUnits<DXRelationDefinitionUnit>(x)).ToList().ToList();
        }
    }
}