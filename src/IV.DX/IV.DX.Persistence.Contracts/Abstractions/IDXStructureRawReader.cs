using IV.DX.Kernel.Models;

namespace IV.DX.Persistence.Contracts.Abstractions
{
    internal interface IDXStructureRawReader
    {
        IEnumerable<DXElementDefinitionUnit> LoadDXElementInfosRaw();
        IEnumerable<DXUnitDefinitionUnit> LoadDXUnitInfosRaw();
        IEnumerable<DXEnumDefinitionUnit> LoadDXEnumInfosRaw();
        IEnumerable<DXRelationDefinitionUnit> LoadDXRelationInfosRaw();
    }
}