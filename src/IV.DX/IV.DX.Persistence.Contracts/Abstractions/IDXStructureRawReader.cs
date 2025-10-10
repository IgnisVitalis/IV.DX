using IV.DX.Kernel.Models;

namespace IV.DX.Persistence.Contracts.Abstractions
{
    internal interface IDXStructureRawReader
    {
        IEnumerable<DXElementDefinitionUnit> LoadBlockInfosRaw();
        IEnumerable<DXUnitDefinitionUnit> LoadEntityInfosRaw();
        IEnumerable<DXEnumDefinitionUnit> LoadEnumInfosRaw();
        IEnumerable<DXRelationDefinitionUnit> LoadRelationInfosRaw();
    }
}
