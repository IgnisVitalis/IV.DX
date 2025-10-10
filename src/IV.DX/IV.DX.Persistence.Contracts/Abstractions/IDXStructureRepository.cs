using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;

namespace IV.DX.Persistence.Contracts.Abstractions
{
    internal interface IDXStructureRepository
    {
        void CreateDataStructure(DXObjectDefinitionUnit dataBlock);
        void UpdatedDataStructure(DXObjectDefinitionUnit dataBlock);
        void DropDataStructure(DXObjectDefinitionUnit dataBlock);
        void CreateDataStructure(DXRelationDefinitionUnit entity);
        void DropDataStructure(DXRelationDefinitionUnit entity);
        void CreateDataStructure(DXUnitDefinitionUnit obj, DXElementDefinitionUnit block);
        void DropDataStructure(DXUnitDefinitionUnit obj, DXElementDefinitionUnit block);
        void SetEntityInheritance(string childEntity, string baseEntity);
        DXUnitDefinitionUnit GetBaseEntity(DXUnitDefinitionUnit derivedEntity);
        DXUnitDefinitionUnit GetEntity(string name);
        DXEnumDefinitionUnit GetEnum(string enumName);
        IEnumerable<DXElementDefinitionUnit> GetBlocks(IEnumerable<Guid> ids);
        DXElementDefinitionUnit GetBlock(Guid id);
        IEnumerable<DXElementDefinitionUnit> GetRelatedBlocks(DXUnitDefinitionUnit entity, DXElementInUnitTypeEnum relationType);
        IEnumerable<DXElementDefinitionUnit> GetRelatedBlocks(DXUnitDefinitionUnit entity);
        DXRelationDefinitionUnit GetRelation(string objectNameLeft, string relationNameLeft, string objectNameRight, string relationNameRight);
        IEnumerable<DXEnumDefinitionUnit> GetEnums(IEnumerable<Guid> ids);
        DXEnumDefinitionUnit GetEnum(Guid id);
    }
}