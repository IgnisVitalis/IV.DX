using IV.DX.Kernel.Converters.DXModelDefinitionConverters;
using IV.DX.Kernel.Converters.DXObjectConverters;
using IV.DX.Kernel.Migration.Models;
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

            var dxUnitInheritance = _dxStructureCache.GetDXUnitInheritance("DXElementDefinitionUnit");

            var dxModeDefinition = DXDataSetDefinitionConverter.ToDXModelDefinition<DXElementDefinitionUnit>(dxUnitInheritance);

            var block = this.GetItemsRecord(dxModeDefinition, DXLoadingType.Full);

            var dxElementInfos = DXRecordConverter.ToDXUnits<DXElementDefinitionUnit>(new[] { block }).ToList();

            return dxElementInfos;
        }

        public IEnumerable<DXUnitDefinitionUnit> LoadDXUnitInfosRaw()
        {
            if (DXMaintenanceToken.IsCoreInitializing)
                return DXUnitDefinitionUnitItems.Items;

            var dxUnitInheritance = _dxStructureCache.GetDXUnitInheritance("DXUnitDefinitionUnit");

            var dxModeDefinition = DXDataSetDefinitionConverter.ToDXModelDefinition<DXUnitDefinitionUnit>(dxUnitInheritance);

            var block = this.GetItemsRecord(dxModeDefinition, DXLoadingType.Full);

            var dxUnitInfos = DXRecordConverter.ToDXUnits<DXUnitDefinitionUnit>(new[] { block }).ToList();

            return dxUnitInfos;
        }

        public IEnumerable<DXEnumDefinitionUnit> LoadDXEnumInfosRaw()
        {
            if (DXMaintenanceToken.IsCoreInitializing)
                return DXEnumDefinitionUnitItems.Items;

            var dxUnitInheritance = _dxStructureCache.GetDXUnitInheritance("DXEnumDefinitionUnit");

            var dxModeDefinition = DXDataSetDefinitionConverter.ToDXModelDefinition<DXEnumDefinitionUnit>(dxUnitInheritance);

            var block = this.GetItemsRecord(dxModeDefinition, DXLoadingType.Full);

            var enumInfos = DXRecordConverter.ToDXUnits<DXEnumDefinitionUnit>(new[] { block }).ToList();

            return enumInfos;
        }

        public IEnumerable<DXRelationDefinitionUnit> LoadDXRelationInfosRaw()
        {
            if (DXMaintenanceToken.IsCoreInitializing)
                //return Enumerable.Empty<DXRelationDefinitionUnit>();
                return DXRelationDefinitionUnitItems.Items;



            // Enumerable.Empty<DXRelationDefinitionUnit>();

            var dxUnitInheritance = _dxStructureCache.GetDXUnitInheritance("DXRelationDefinitionUnit");
            try
            {
                var dxModeDefinition = DXDataSetDefinitionConverter.ToDXModelDefinition<DXRelationDefinitionUnit>(dxUnitInheritance);
                var block = this.GetItemsRecord(dxModeDefinition, DXLoadingType.Full);

                return DXRecordConverter.ToDXUnits<DXRelationDefinitionUnit>(new[] { block }).ToList();
            }
            catch (InvalidOperationException ex)
                when (ex.Message.Contains("not found for type 'DXRelationDefinitionUnit'", StringComparison.Ordinal))
            {
                var dxModeDefinition = "DXRelationDefinitionUnit".ToDXModelDefinition(dxUnitInheritance);
                var block = this.GetItemsRecord(dxModeDefinition, DXLoadingType.Full);

                return DXRecordConverter.ToDXUnits<DXRelationDefinitionUnit>(new[] { block }).ToList();
            }
        }
    }
}
