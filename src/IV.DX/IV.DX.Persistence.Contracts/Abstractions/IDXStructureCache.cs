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

        DXUnitInheritance GetDXUnitInheritance(string dxUnitTypeName);
        // IEnumerable<DXUnitDefinitionUnit> GetHierarchyChainOfBaseEntitiesFromDerivedToBase(string derivedDXUnitTypeName);
        DXUnitInheritance GetDXUnitInheritance(DXUnitDefinitionUnit dxUnitType);
        // IEnumerable<DXUnitDefinitionUnit> GetHierarchyChainOfBaseEntitiesFromDerivedToBase(DXUnitDefinitionUnit derivedDXUnit);

        HashSet<DXElementDefinitionUnit> GetRelatedDXElementDefinitions(DXUnitDefinitionUnit dxUnit);
        HashSet<DXElementDefinitionUnit> GetRelatedDXElementDefinitions(DXUnitDefinitionUnit dxUnit, DXElementInUnitTypeEnum relationType);
        DXUnitDefinitionUnit? GetBaseDXUnit(DXUnitDefinitionUnit derivedDXUnit);

        int Version { get; }
        Task RefreshAsync(CancellationToken ct = default);
    }
}