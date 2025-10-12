using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;

namespace IV.DX.Persistence.Contracts.Abstractions
{
    internal interface IDXStructureRepository
    {
        void CreateDataStructure(DXObjectDefinitionUnit dxElementDefinition);
        void UpdatedDataStructure(DXObjectDefinitionUnit dxElementDefinition);
        void DropDataStructure(DXObjectDefinitionUnit dxElementDefinition);
        void CreateDataStructure(DXRelationDefinitionUnit dxRelationDefinition);
        void DropDataStructure(DXRelationDefinitionUnit dxRelationDefinition);
        void CreateDataStructure(DXUnitDefinitionUnit dxUnitDefinition, DXElementDefinitionUnit dxElementDefinition);
        void DropDataStructure(DXUnitDefinitionUnit dxUnitDefinition, DXElementDefinitionUnit dxElementDefinition);
        void SetDXUnitInheritance(string childEntity, string baseEntity);
        DXUnitDefinitionUnit GetBaseDXUnit(DXUnitDefinitionUnit derivedEntity);
        DXUnitDefinitionUnit GetDXUnitDefinition(string name);
        DXEnumDefinitionUnit GetDXEnumDefinition(string enumName);
        IEnumerable<DXElementDefinitionUnit> GetDXElementDefinitions(IEnumerable<Guid> ids);
        DXElementDefinitionUnit GetBlock(Guid id);
        IEnumerable<DXElementDefinitionUnit> GetRelatedBlocks(DXUnitDefinitionUnit dxUnit, DXElementInUnitTypeEnum relationType);
        IEnumerable<DXElementDefinitionUnit> GetRelatedBlocks(DXUnitDefinitionUnit dxUnit);
        DXRelationDefinitionUnit GetRelation(string objectNameLeft, string relationNameLeft, string objectNameRight, string relationNameRight);
        IEnumerable<DXEnumDefinitionUnit> GetEnums(IEnumerable<Guid> ids);
        DXEnumDefinitionUnit GetEnum(Guid id);
    }
}