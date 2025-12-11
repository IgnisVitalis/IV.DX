using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Helpers.DXModelDefinitionHelpers;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Persistence
{
    internal partial class DXCoreRepository : IDXUnitCoreRepository, IDXStructureRepository, IDXEnumCoreRepository, IDXStructureRawReader, IDXElementCoreRepository, IDXRawReader
    {
        IEnumerable<DXModel> IDXEnumCoreRepository.GetItems(string enumType)
        {
            var modelDefinition = this.GetEnumModelDefinition(enumType);

            if (modelDefinition == null)
                return null;

            return this.GetItems(modelDefinition, DXLoadingType.Full);
        }

        private DXModelDefinition GetEnumModelDefinition(string type)
        {
            var mainDXUnit = this.GetDXEnumDefinition(type);

            if (mainDXUnit == null)
                return null;

            var modelDefinition = DXModelDefinitionHelper.BuildModelDefinition(mainDXUnit, _dxStructureCache.DXRelations);

            return modelDefinition;
        }
    }
}
