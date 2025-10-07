using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;

namespace IV.DX.Persistence.Contracts.Abstractions
{
    internal interface IDataStructureRepository
    {
        IEnumerable<DPRelationObject> RelationInfos { get; }
        IEnumerable<DXUnitDefinitionUnit> EntityInfos { get; }
        IEnumerable<DXElementDefinitionUnit> BlockInfos { get; }
        IEnumerable<DXEnumDefinitionUnit> EnumInfos { get; }
        void UpdateCache();
        void CreateDataStructure(DPObjectDescObject dataBlock);
        void UpdatedDataStructure(DPObjectDescObject dataBlock);
        void DropDataStructure(DPObjectDescObject dataBlock);
        void CreateDataStructure(DPRelationObject entity);
        void DropDataStructure(DPRelationObject entity);
        void CreateDataStructure(DXUnitDefinitionUnit obj, DXElementDefinitionUnit block);
        void DropDataStructure(DXUnitDefinitionUnit obj, DXElementDefinitionUnit block);
        void SetEntityInheritance(string childEntity, string baseEntity);
        DXUnitDefinitionUnit GetBaseEntity(DXUnitDefinitionUnit derivedEntity);
        DXUnitDefinitionUnit GetEntity(string name);
        DXEnumDefinitionUnit GetEnum(string enumName);
        IEnumerable<DXElementDefinitionUnit> GetBlocks(IEnumerable<Guid> ids);
        DXElementDefinitionUnit GetBlock(Guid id);
        IEnumerable<DXElementDefinitionUnit> GetRelatedBlocks(DXUnitDefinitionUnit entity, DPBlockInObjectTypeEnum relationType);
        IEnumerable<DXElementDefinitionUnit> GetRelatedBlocks(DXUnitDefinitionUnit entity);
        DPRelationObject GetRelation(string objectNameLeft, string relationNameLeft, string objectNameRight, string relationNameRight);
        IEnumerable<DXEnumDefinitionUnit> GetEnums(IEnumerable<Guid> ids);
        DXEnumDefinitionUnit GetEnum(Guid id);
    }
}