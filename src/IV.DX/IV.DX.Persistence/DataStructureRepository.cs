using IV.DX.Kernel.Converters;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using IV.DX.Persistence.CoreData;
using System.Diagnostics.CodeAnalysis;

namespace IV.DX.Persistence
{
    public partial class CoreRepository : ICoreRepository, IDataStructureRepository, IEnumCoreRepository
    {
        private IList<DPRelationObject> _relationInfos;
        public IEnumerable<DPRelationObject> RelationInfos { get { return this._relationInfos; } }

        private IList<DPEntityDescObject> _entityInfos;
        public IEnumerable<DPEntityDescObject> EntityInfos { get { return this._entityInfos; } }

        private IList<DPBlockDescObject> _blockInfos;
        public IEnumerable<DPBlockDescObject> BlockInfos { get { return this._blockInfos; } }

        private IList<DPEnumDescObject> _enumInfos;
        public IEnumerable<DPEnumDescObject> EnumInfos { get { return this._enumInfos; } }

        public void CreateDataStructure(DPObjectDescObject dataBlock)
        {
            var sqlQuery = this._queryHelper.GetSQLQueryToCreateTable(dataBlock);

            this._queryHelper.RunSQLQuery(this._connectionStr, sqlQuery);

            this.UpdateCache();
        }

        public void CreateDataStructure(DPRelationObject entity)
        {
            var sqlQuery = this.GetSQLQueryToCreateRelation(entity);

            this._queryHelper.RunSQLQuery(this._connectionStr, sqlQuery);

            this.UpdateCache();
        }

        public void DropDataStructure(DPRelationObject entity)
        {
            var sqlQuery = this.GetSQLQueryToDeleteRelation(entity);

            this._queryHelper.RunSQLQuery(this._connectionStr, sqlQuery);

            this.UpdateCache();
        }

        public void CreateDataStructure(DPEntityDescObject obj, DPBlockDescObject block)
        {
            var sqlQuery = this.GetSQLQueryToCreateTable(obj, block);

            this._queryHelper.RunSQLQuery(this._connectionStr, sqlQuery);

            this.UpdateCache();
        }

        public void DropDataStructure(DPEntityDescObject obj, DPBlockDescObject block)
        {
            var sqlQuery = this.GetSQLQueryToDropTable(obj, block);

            this._queryHelper.RunSQLQuery(this._connectionStr, sqlQuery);

            this.UpdateCache();
        }

        public void UpdatedDataStructure(DPObjectDescObject dataBlock)
        {
            var result = this.GetItem(ModelConverter.GetESQLModelDefinition(typeof(DPObjectDescObject)), dataBlock.ID, TypeOfEntityLoading.Full);
            var existingDataBlock = ESQLObjectHelper.CreateInstance<DPObjectDescObject>(result);

            var sqlQuery = this._queryHelper.GetSQLQueryToAlterTable(dataBlock, existingDataBlock);

            this._queryHelper.RunSQLQuery(this._connectionStr, sqlQuery);

            this.UpdateCache();
        }

        public void DropDataStructure(DPObjectDescObject dataBlock)
        {
            var sqlQuery = this._queryHelper.GetSQLQueryToDropTable(dataBlock);
            this._queryHelper.RunSQLQuery(this._connectionStr, sqlQuery);

            this.UpdateCache();
        }

        private string GetSQLQueryToDropTable(DPEntityDescObject obj, DPBlockDescObject block)
        {
            return this._queryHelper.GetSQLQueryToDropTable(obj, block);
        }

        private string GetSQLQueryToCreateTable(DPEntityDescObject obj, DPBlockDescObject block)
        {
            return this._queryHelper.GetSQLQueryToCreateTable(obj, block);
        }

        private string GetSQLQueryToCreateRelation(DPRelationObject obj)
        {
            string result = "";

            switch (obj.DPRelationGenBlock.RelationType)
            {
                case DPRelationTypeEnum.ManyToMany: result = this._queryHelper.GetSQLQueryToCreateRelationManyToMany(obj, this._connectionStr); break;
                case DPRelationTypeEnum.ManyToOne: result = this.GetSQLQueryToCreateRelationManyToOne(obj); break;
                case DPRelationTypeEnum.ManyToZeroOne: result = this.GetSQLQueryToCreateRelationManyToZeroOne(obj); break;
                case DPRelationTypeEnum.OneToMany: result = this.GetSQLQueryToCreateRelationOneToMany(obj); break;
                case DPRelationTypeEnum.OneToZeroOne: result = this.GetSQLQueryToCreateRelationOneToZeroOne(obj); break;
                case DPRelationTypeEnum.ZeroOneToMany: result = this.GetSQLQueryToCreateRelationZeroOneToMany(obj); break;
                case DPRelationTypeEnum.ZeroOneToOne: result = this.GetSQLQueryToCreateRelationZeroOneToOne(obj); break;
                case DPRelationTypeEnum.ZeroOneToZeroOne: result = this.GetSQLQueryToCreateRelationZeroOneToZeroOne(obj); break;
            }

            return result;
        }

        private string GetSQLQueryToCreateRelationManyToOne(DPRelationObject obj)
        {
            return this._queryHelper.GetSQLQueryToCreateRelationManyTo(obj, false, false);
        }

        private string GetSQLQueryToCreateRelationManyToZeroOne(DPRelationObject obj)
        {
            return this._queryHelper.GetSQLQueryToCreateRelationManyTo(obj, true, false);
        }

        private string GetSQLQueryToCreateRelationZeroOneToOne(DPRelationObject obj)
        {
            return this._queryHelper.GetSQLQueryToCreateRelationManyTo(obj, false, true);
        }

        private string GetSQLQueryToCreateRelationOneToMany(DPRelationObject obj)
        {
            return this._queryHelper.GetSQLQueryToCreateRelationToMany(obj, false, false);
        }

        private string GetSQLQueryToCreateRelationZeroOneToMany(DPRelationObject obj)
        {
            return this._queryHelper.GetSQLQueryToCreateRelationToMany(obj, true, false);
        }

        private string GetSQLQueryToCreateRelationOneToZeroOne(DPRelationObject obj)
        {
            return this._queryHelper.GetSQLQueryToCreateRelationToMany(obj, false, true);
        }

        private string GetSQLQueryToCreateRelationZeroOneToZeroOne(DPRelationObject obj)
        {
            obj.DPRelationGenBlock.RelationTable = obj.DPRelationGenBlock.ObjectNameRight;

            return this._queryHelper.GetSQLQueryToCreateRelationToMany(obj, true, true);
        }

        private string GetSQLQueryToDeleteRelation(DPRelationObject obj)
        {
            string result = "";

            switch (obj.DPRelationGenBlock.RelationType)
            {
                case DPRelationTypeEnum.ManyToMany: result = this.GetSQLQueryToDeleteRelationManyToMany(obj); break;
                case DPRelationTypeEnum.ManyToOne: result = this._queryHelper.GetSQLQueryToDeleteRelationManyToOne(obj); break;
                case DPRelationTypeEnum.ManyToZeroOne: result = this.GetSQLQueryToDeleteRelationManyToZeroOne(obj); break;
                case DPRelationTypeEnum.OneToMany: result = this._queryHelper.GetSQLQueryToDeleteRelationOneToMany(obj); break;
                case DPRelationTypeEnum.OneToZeroOne: result = this._queryHelper.GetSQLQueryToDeleteRelationOneToZeroOne(obj); break;
                case DPRelationTypeEnum.ZeroOneToMany: result = this.GetSQLQueryToDeleteRelationZeroOneToMany(obj); break;
                case DPRelationTypeEnum.ZeroOneToOne: result = this._queryHelper.GetSQLQueryToDeleteRelationZeroOneToOne(obj); break;
                case DPRelationTypeEnum.ZeroOneToZeroOne: result = this._queryHelper.GetSQLQueryToDeleteRelationOneToZeroOne(obj); break;
            }

            return result;
        }

        private string GetSQLQueryToDeleteRelationManyToMany(DPRelationObject entity)
        {
            string relationTableName;

            if (string.IsNullOrEmpty(entity.DPRelationGenBlock.RelationTable))
            {
                var existingModel = this.GetItem(ModelConverter.GetESQLModelDefinition(typeof(DPRelationObject)), entity.ID, TypeOfEntityLoading.Full);

                var existingEntity = ESQLObjectHelper.CreateInstance<DPRelationObject>(existingModel);

                relationTableName = existingEntity.DPRelationGenBlock.RelationTable;
            }
            else
            {
                relationTableName = entity.DPRelationGenBlock.RelationTable;
            }

            return this._queryHelper.GetSQLQueryToDropTable(relationTableName);
        }

        private string GetSQLQueryToDeleteRelationManyToZeroOne(DPRelationObject obj)
        {
            return this._queryHelper.GetSQLQueryToDeleteRelationManyToOne(obj);
        }

        private string GetSQLQueryToDeleteRelationZeroOneToMany(DPRelationObject obj)
        {
            return this._queryHelper.GetSQLQueryToDeleteRelationOneToMany(obj);
        }

        public void SetEntityInheritance(string childEntity, string baseEntity)
        {
            var query = this._queryHelper.GetQueryToSetEntityInheritance(childEntity, baseEntity);

            this._queryHelper.RunSQLQuery(this._connectionStr, query);

            this.UpdateCache();
        }

        public DPEntityDescObject GetBaseEntity(DPEntityDescObject derivedEntity)
        {
            if (derivedEntity == null || derivedEntity.DPEntityInheritanceBlock?.BaseEntity == null)
                return null;

            var result = EntityInfos.SingleOrDefault(x => x.ID == derivedEntity.DPEntityInheritanceBlock.BaseEntity);

            if (result == null)
            {
                this.UpdateCache();

                result = EntityInfos.SingleOrDefault(x => x.ID == derivedEntity.DPEntityInheritanceBlock.BaseEntity);
            }

            return result;
        }

        public DPEntityDescObject GetEntity(string entityType)
        {
            var result = EntityInfos.SingleOrDefault(x => x.DPObjectDescGenBlock.Name.Equals(entityType));

            if (result == null)
            {
                this.UpdateCache();

                result = EntityInfos.SingleOrDefault(x => x.DPObjectDescGenBlock.Name.Equals(entityType));
            }

            return result;
        }

        public IEnumerable<DPBlockDescObject> GetRelatedBlocks(DPEntityDescObject entity, DPBlockInObjectTypeEnum relationType)
        {
            if (entity.DPBlockInEntityDescGenBlock == null)
                return null;

            var relatedBlockIds =
              entity.DPBlockInEntityDescGenBlock
              .Announced
              .Where(x => x.RelationType == relationType)
              .Select(x => x.DPBlockDescObject).ToList();

            var relatedBlocks = BlockInfos.Where(x => relatedBlockIds.Contains(x.ID)).ToList();

            return relatedBlocks;
        }

        public IEnumerable<DPBlockDescObject> GetRelatedBlocks(DPEntityDescObject entity)
        {
            if (entity.DPBlockInEntityDescGenBlock == null)
                return null;

            var relatedBlockIds =
                entity.DPBlockInEntityDescGenBlock
                .Announced
                .Select(x => x.DPBlockDescObject).ToList();

            var relatedBlocks = BlockInfos.Where(x => relatedBlockIds.Contains(x.ID)).ToList();

            return relatedBlocks;
        }

        private void LoadEnumInfos()
        {
            var enumsModelsFromDB = this.GetItems(DPEnumDescObject.ESQLModelDefinition, TypeOfEntityLoading.Full);

            var enumInfos = enumsModelsFromDB.Select(x => ESQLObjectHelper.CreateInstance<DPEnumDescObject>(x));

            var enumInfosWithoutCore = enumInfos.Except(CoreDataStructureRepository.CoreEnumInfos, DPObjectDescObjectIDComparer.Instance)
                .Select(x => x as DPEnumDescObject);

            this._enumInfos = CoreDataStructureRepository.CoreEnumInfos.Concat(enumInfosWithoutCore).ToList();
        }

        private void LoadEntityInfos()
        {
            var entityModelsFromDB = this.GetItems(DPEntityDescObject.ESQLModelDefinition, TypeOfEntityLoading.Full);

            var entityInfos = entityModelsFromDB.Select(x => ESQLObjectHelper.CreateInstance<DPEntityDescObject>(x));

            var entityInfosWithoutCore = entityInfos.Except(CoreDataStructureRepository.CoreEntityInfos, DPObjectDescObjectIDComparer.Instance)
                .Select(x => x as DPEntityDescObject);

            this._entityInfos = CoreDataStructureRepository.CoreEntityInfos.Concat(entityInfosWithoutCore).ToList();
        }

        private void LoadRelationInfos()
        {
            var result = this.GetItems(DPRelationObject.ESQLModelDefinition, TypeOfEntityLoading.Full);
            this._relationInfos = result.Select(x => ESQLObjectHelper.CreateInstance<DPRelationObject>(x)).ToList();
        }

        private void LoadBlockInfos()
        {
            var blockModelsFromDB = this.GetItems(DPBlockDescObject.ESQLModelDefinition, TypeOfEntityLoading.Full);

            var blockInfos = blockModelsFromDB.Select(x => ESQLObjectHelper.CreateInstance<DPBlockDescObject>(x));

            var blockInfosWithoutCore = blockInfos.Except(CoreDataStructureRepository.CoreBlockInfos, DPObjectDescObjectIDComparer.Instance)
                .Select(x => x as DPBlockDescObject);

            this._blockInfos = CoreDataStructureRepository.CoreBlockInfos.Concat(blockInfosWithoutCore).ToList();
        }

        private class DPObjectDescObjectIDComparer : IEqualityComparer<DPObjectDescObject>
        {
            public static DPObjectDescObjectIDComparer Instance { get; set; } = new DPObjectDescObjectIDComparer();

            private DPObjectDescObjectIDComparer()
            {

            }

            public bool Equals(DPObjectDescObject x, DPObjectDescObject y)
            {
                if (x == null)
                    return false;

                if (y == null)
                    return false;

                if (x.GetType() != y.GetType())
                    return false;

                return x.ID == y.ID;
            }

            public int GetHashCode([DisallowNull] DPObjectDescObject obj)
            {
                return obj.ID.GetHashCode();
            }
        }

        public DPRelationObject GetRelation(string objectNameLeft, string relationNameLeft, string objectNameRight, string relationNameRight)
        {
            var existingRelation = this.RelationInfos.SingleOrDefault(x =>
                x.DPRelationGenBlock.ObjectNameLeft == objectNameLeft
                && x.DPRelationGenBlock.RelationNameLeft == relationNameLeft
                && x.DPRelationGenBlock.ObjectNameRight == objectNameRight
                && x.DPRelationGenBlock.RelationNameRight == relationNameRight);

            if (existingRelation == null)
            {
                this.UpdateCache();

                existingRelation = this.RelationInfos.SingleOrDefault(x =>
                x.DPRelationGenBlock.ObjectNameLeft == objectNameLeft
                && x.DPRelationGenBlock.RelationNameLeft == relationNameLeft
                && x.DPRelationGenBlock.ObjectNameRight == objectNameRight
                && x.DPRelationGenBlock.RelationNameRight == relationNameRight);
            }

            return existingRelation;
        }

        public IEnumerable<DPBlockDescObject> GetBlocks(IEnumerable<Guid> ids)
        {
            if (ids == null)
                return null;

            var result = ids.Select(x => this.GetBlock(x)).Where(x => x != null).ToList();

            return result;
        }

        public DPBlockDescObject GetBlock(Guid id)
        {
            var existingBlock = this.BlockInfos.SingleOrDefault(x => x.ID == id);

            if (existingBlock == null)
            {
                this.UpdateCache();

                existingBlock = this.BlockInfos.SingleOrDefault(x => x.ID == id);
            }

            return existingBlock;
        }

        public IEnumerable<DPEnumDescObject> GetEnums(IEnumerable<Guid> ids)
        {
            if (ids == null)
                return null;

            var result = ids.Select(x => this.GetEnum(x)).Where(x => x != null).ToList();

            return result;
        }

        public DPEnumDescObject GetEnum(Guid id)
        {
            var existingEnum = this.EnumInfos.SingleOrDefault(x => x.ID == id);

            if (existingEnum == null)
            {
                this.UpdateCache();

                existingEnum = this.EnumInfos.SingleOrDefault(x => x.ID == id);
            }

            return existingEnum;
        }

        public DPEnumDescObject GetEnum(string enumName)
        {
            var existingEnum = this.EnumInfos.SingleOrDefault(x => x.DPObjectDescGenBlock.Name.Equals(enumName));

            if (existingEnum == null)
            {
                this.UpdateCache();

                existingEnum = this.EnumInfos.SingleOrDefault(x => x.DPObjectDescGenBlock.Name.Equals(enumName));
            }

            return existingEnum;
        }

        public void UpdateCache()
        {
            if (MaintenanceToken.IsCoreInitializing)
            {
                this._blockInfos = DPBlockDescObjectItems.Items;
                this._entityInfos = DPEntityDescObjectItems.Items;
                this._enumInfos = DPEnumDescObjectItems.Items;
            }
            else
            {
                this.LoadEnumInfos();
                this.LoadBlockInfos();
                this.LoadEntityInfos();
                this.LoadRelationInfos();
            }
        }
    }
}