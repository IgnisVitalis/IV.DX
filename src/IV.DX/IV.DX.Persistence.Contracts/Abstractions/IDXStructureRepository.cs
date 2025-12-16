using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;

namespace IV.DX.Persistence.Contracts.Abstractions
{
    internal interface IDXStructureRepository
    {
        void CreateDataStructure(DXObjectDefinitionUnit dxObjectDefinition);
        void UpdateUniqueColumns(DXObjectDefinitionUnit dxObjectDefinition);
        void UpdatedDataStructure(DXObjectDefinitionUnit dxObjectDefinition);
        void DropDataStructure(DXObjectDefinitionUnit dxObjectDefinition);
        void CreateDataStructure(DXRelationDefinitionUnit dxRelationDefinition);
        void DropDataStructure(DXRelationDefinitionUnit dxRelationDefinition);
        void CreateDataStructure(DXUnitDefinitionUnit dxUnitDefinition, DXElementDefinitionUnit dxElementDefinition);
        void DropDataStructure(DXUnitDefinitionUnit dxUnitDefinition, DXElementDefinitionUnit dxElementDefinition);
        void SetDXUnitInheritance(string childDXUnit, string baseDXUnit);
        DXUnitDefinitionUnit? GetBaseDXUnit(DXUnitDefinitionUnit derivedDXUnit);
        DXUnitDefinitionUnit? GetDXUnitDefinition(string name);
        DXUnitDefinitionUnit GetDXUnitDefinition(Guid id);
        IEnumerable<DXUnitDefinitionUnit> GetDXUnitDefinitions(IEnumerable<Guid> ids);
        DXEnumDefinitionUnit GetDXEnumDefinition(string enumName);
        IEnumerable<DXElementDefinitionUnit> GetDXElementDefinitions(IEnumerable<Guid> ids);
        DXElementDefinitionUnit GetDXElementDefinition(Guid id);
        IEnumerable<DXElementDefinitionUnit> GetRelatedDXElementDefinitions(DXUnitDefinitionUnit dxUnit, DXElementInUnitTypeEnum relationType);
        IEnumerable<DXElementDefinitionUnit> GetRelatedDXElementDefinitions(DXUnitDefinitionUnit dxUnit);
        DXRelationDefinitionUnit GetDXRelationDefinition(string objectNameLeft, string relationNameLeft, string objectNameRight, string relationNameRight);
        IEnumerable<DXEnumDefinitionUnit> GetDXEnumDefinitions(IEnumerable<Guid> ids);
        DXEnumDefinitionUnit GetDXEnumDefinition(Guid id);
    }
}