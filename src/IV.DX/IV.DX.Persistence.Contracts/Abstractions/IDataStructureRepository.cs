using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;

namespace IV.DX.Persistence.Contracts.Abstractions
{
    public interface IDataStructureRepository
    {
        IEnumerable<DPRelationObject> RelationInfos { get; }
        IEnumerable<DPEntityDescObject> EntityInfos { get; }
        IEnumerable<DPBlockDescObject> BlockInfos { get; }
        IEnumerable<DPEnumDescObject> EnumInfos { get; }
        void UpdateCache();
        void CreateDataStructure(DPObjectDescObject dataBlock);
        void UpdatedDataStructure(DPObjectDescObject dataBlock);
        void DropDataStructure(DPObjectDescObject dataBlock);
        void CreateDataStructure(DPRelationObject entity);
        void DropDataStructure(DPRelationObject entity);
        void CreateDataStructure(DPEntityDescObject obj, DPBlockDescObject block);
        void DropDataStructure(DPEntityDescObject obj, DPBlockDescObject block);
        void SetEntityInheritance(string childEntity, string baseEntity);
        DPEntityDescObject GetBaseEntity(DPEntityDescObject derivedEntity);
        DPEntityDescObject GetEntity(string name);
        DPEnumDescObject GetEnum(string enumName);
        IEnumerable<DPBlockDescObject> GetBlocks(IEnumerable<Guid> ids);
        DPBlockDescObject GetBlock(Guid id);
        IEnumerable<DPBlockDescObject> GetRelatedBlocks(DPEntityDescObject entity, DPBlockInObjectTypeEnum relationType);
        IEnumerable<DPBlockDescObject> GetRelatedBlocks(DPEntityDescObject entity);
        DPRelationObject GetRelation(string objectNameLeft, string relationNameLeft, string objectNameRight, string relationNameRight);
        IEnumerable<DPEnumDescObject> GetEnums(IEnumerable<Guid> ids);
        DPEnumDescObject GetEnum(Guid id);
    }
}