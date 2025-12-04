using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;

namespace IV.DX.Persistence.Contracts.Abstractions
{
    internal interface IDXStructureCache
    {
        IReadOnlyList<DXElementDefinitionUnit> DXElements { get; }
        IReadOnlyList<DXUnitDefinitionUnit> DXUnits { get; }
        IReadOnlyList<DXEnumDefinitionUnit> DXEnums { get; }
        IReadOnlyList<DXRelationDefinitionUnit> DXRelations { get; }

        DXEnumDefinitionUnit GetDXEnum(string name);
        DXUnitDefinitionUnit GetDXUnit(string name);
        DXElementDefinitionUnit GetDXElement(string name);
        IEnumerable<DXRelationDefinitionUnit> GetDXRelations(string name);    

        DXElementInUnitTypeEnum GetElementInUnitRelationType(string dxUnitTypeName, string dxElementTypeName);

        int Version { get; }
        Task RefreshAsync(CancellationToken ct = default);
    }
}