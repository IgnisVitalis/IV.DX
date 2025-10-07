using IV.DX.Kernel.Converters;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using IV.DX.Persistence.CoreData;
using System.Diagnostics.CodeAnalysis;

namespace IV.DX.Persistence
{
    internal partial class CoreRepository : ICoreRepository, IDataStructureRepository, IEnumCoreRepository
    {
        private IList<DXRelationDefinitionUnit> _relationInfos;
        public IEnumerable<DXRelationDefinitionUnit> RelationInfos { get { return this._relationInfos; } }

        private IList<DXUnitDefinitionUnit> _entityInfos;
        public IEnumerable<DXUnitDefinitionUnit> EntityInfos { get { return this._entityInfos; } }

        private IList<DXElementDefinitionUnit> _blockInfos;
        public IEnumerable<DXElementDefinitionUnit> BlockInfos { get { return this._blockInfos; } }

        private IList<DXEnumDefinitionUnit> _enumInfos;
        public IEnumerable<DXEnumDefinitionUnit> EnumInfos { get { return this._enumInfos; } }

        public void CreateDataStructure(DPObjectDescObject dataBlock)
        {
            var sqlQuery = this._queryHelper.GetSQLQueryToCreateTable(dataBlock);

            this._queryHelper.RunSQLQuery(this._connectionStr, sqlQuery);

            this.UpdateCache();
        }

        public void CreateDataStructure(DXRelationDefinitionUnit entity)
        {
            var sqlQuery = this.GetSQLQueryToCreateRelation(entity);

            this._queryHelper.RunSQLQuery(this._connectionStr, sqlQuery);

            this.UpdateCache();
        }

        public void DropDataStructure(DXRelationDefinitionUnit entity)
        {
            var sqlQuery = this.GetSQLQueryToDeleteRelation(entity);

            this._queryHelper.RunSQLQuery(this._connectionStr, sqlQuery);

            this.UpdateCache();
        }

        public void CreateDataStructure(DXUnitDefinitionUnit obj, DXElementDefinitionUnit block)
        {
            var sqlQuery = this.GetSQLQueryToCreateTable(obj, block);

            this._queryHelper.RunSQLQuery(this._connectionStr, sqlQuery);

            this.UpdateCache();
        }

        public void DropDataStructure(DXUnitDefinitionUnit obj, DXElementDefinitionUnit block)
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

        private string GetSQLQueryToDropTable(DXUnitDefinitionUnit obj, DXElementDefinitionUnit block)
        {
            return this._queryHelper.GetSQLQueryToDropTable(obj, block);
        }

        private string GetSQLQueryToCreateTable(DXUnitDefinitionUnit obj, DXElementDefinitionUnit block)
        {
            return this._queryHelper.GetSQLQueryToCreateTable(obj, block);
        }

        private string GetSQLQueryToCreateRelation(DXRelationDefinitionUnit obj)
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

        private string GetSQLQueryToCreateRelationManyToOne(DXRelationDefinitionUnit obj)
        {
            return this._queryHelper.GetSQLQueryToCreateRelationManyTo(obj, false, false);
        }

        private string GetSQLQueryToCreateRelationManyToZeroOne(DXRelationDefinitionUnit obj)
        {
            return this._queryHelper.GetSQLQueryToCreateRelationManyTo(obj, true, false);
        }

        private string GetSQLQueryToCreateRelationZeroOneToOne(DXRelationDefinitionUnit obj)
        {
            return this._queryHelper.GetSQLQueryToCreateRelationManyTo(obj, false, true);
        }

        private string GetSQLQueryToCreateRelationOneToMany(DXRelationDefinitionUnit obj)
        {
            return this._queryHelper.GetSQLQueryToCreateRelationToMany(obj, false, false);
        }

        private string GetSQLQueryToCreateRelationZeroOneToMany(DXRelationDefinitionUnit obj)
        {
            return this._queryHelper.GetSQLQueryToCreateRelationToMany(obj, true, false);
        }

        private string GetSQLQueryToCreateRelationOneToZeroOne(DXRelationDefinitionUnit obj)
        {
            return this._queryHelper.GetSQLQueryToCreateRelationToMany(obj, false, true);
        }

        private string GetSQLQueryToCreateRelationZeroOneToZeroOne(DXRelationDefinitionUnit obj)
        {
            obj.DPRelationGenBlock.RelationTable = obj.DPRelationGenBlock.ObjectNameRight;

            return this._queryHelper.GetSQLQueryToCreateRelationToMany(obj, true, true);
        }

        private string GetSQLQueryToDeleteRelation(DXRelationDefinitionUnit obj)
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

        private string GetSQLQueryToDeleteRelationManyToMany(DXRelationDefinitionUnit entity)
        {
            string relationTableName;

            if (string.IsNullOrEmpty(entity.DPRelationGenBlock.RelationTable))
            {
                var existingModel = this.GetItem(ModelConverter.GetESQLModelDefinition(typeof(DXRelationDefinitionUnit)), entity.ID, TypeOfEntityLoading.Full);

                var existingEntity = ESQLObjectHelper.CreateInstance<DXRelationDefinitionUnit>(existingModel);

                relationTableName = existingEntity.DPRelationGenBlock.RelationTable;
            }
            else
            {
                relationTableName = entity.DPRelationGenBlock.RelationTable;
            }

            return this._queryHelper.GetSQLQueryToDropTable(relationTableName);
        }

        private string GetSQLQueryToDeleteRelationManyToZeroOne(DXRelationDefinitionUnit obj)
        {
            return this._queryHelper.GetSQLQueryToDeleteRelationManyToOne(obj);
        }

        private string GetSQLQueryToDeleteRelationZeroOneToMany(DXRelationDefinitionUnit obj)
        {
            return this._queryHelper.GetSQLQueryToDeleteRelationOneToMany(obj);
        }

        public void SetEntityInheritance(string childEntity, string baseEntity)
        {
            var query = this._queryHelper.GetQueryToSetEntityInheritance(childEntity, baseEntity);

            this._queryHelper.RunSQLQuery(this._connectionStr, query);

            this.UpdateCache();
        }

        public DXUnitDefinitionUnit GetBaseEntity(DXUnitDefinitionUnit derivedEntity)
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

        public DXUnitDefinitionUnit GetEntity(string entityType)
        {
            var result = EntityInfos.SingleOrDefault(x => x.DXUnitDefinitionMainElement.Name.Equals(entityType));

            if (result == null)
            {
                this.UpdateCache();

                result = EntityInfos.SingleOrDefault(x => x.DXUnitDefinitionMainElement.Name.Equals(entityType));
            }

            return result;
        }

        public IEnumerable<DXElementDefinitionUnit> GetRelatedBlocks(DXUnitDefinitionUnit entity, DXElementInUnitTypeEnum relationType)
        {
            if (entity.DXElementInUnitDefinitionMainElement == null)
                return null;

            var relatedBlockIds =
              entity.DXElementInUnitDefinitionMainElement
              .Announced
              .Where(x => x.RelationType == relationType)
              .Select(x => x.DXElementDefinitionUnit).ToList();

            var relatedBlocks = BlockInfos.Where(x => relatedBlockIds.Contains(x.ID)).ToList();

            return relatedBlocks;
        }

        public IEnumerable<DXElementDefinitionUnit> GetRelatedBlocks(DXUnitDefinitionUnit entity)
        {
            if (entity.DXElementInUnitDefinitionMainElement == null)
                return null;

            var relatedBlockIds =
                entity.DXElementInUnitDefinitionMainElement
                .Announced
                .Select(x => x.DXElementDefinitionUnit).ToList();

            var relatedBlocks = BlockInfos.Where(x => relatedBlockIds.Contains(x.ID)).ToList();

            return relatedBlocks;
        }

        private void LoadEnumInfos()
        {
            var enumsModelsFromDB = this.GetItems(DXEnumDefinitionUnit.ESQLModelDefinition, TypeOfEntityLoading.Full);

            var enumInfos = enumsModelsFromDB.Select(x => ESQLObjectHelper.CreateInstance<DXEnumDefinitionUnit>(x));

            var enumInfosWithoutCore = enumInfos.Except(CoreDataStructureRepository.CoreEnumInfos, DPObjectDescObjectIDComparer.Instance)
                .Select(x => x as DXEnumDefinitionUnit);

            this._enumInfos = CoreDataStructureRepository.CoreEnumInfos.Concat(enumInfosWithoutCore).ToList();
        }

        private void LoadEntityInfos()
        {
            var entityModelsFromDB = this.GetItems(DXUnitDefinitionUnit.ESQLModelDefinition, TypeOfEntityLoading.Full);

            var entityInfos = entityModelsFromDB.Select(x => ESQLObjectHelper.CreateInstance<DXUnitDefinitionUnit>(x));

            var entityInfosWithoutCore = entityInfos.Except(CoreDataStructureRepository.CoreEntityInfos, DPObjectDescObjectIDComparer.Instance)
                .Select(x => x as DXUnitDefinitionUnit);

            this._entityInfos = CoreDataStructureRepository.CoreEntityInfos.Concat(entityInfosWithoutCore).ToList();
        }

        private void LoadRelationInfos()
        {
            var result = this.GetItems(DXRelationDefinitionUnit.ESQLModelDefinition, TypeOfEntityLoading.Full);
            this._relationInfos = result.Select(x => ESQLObjectHelper.CreateInstance<DXRelationDefinitionUnit>(x)).ToList();
        }

        private void LoadBlockInfos()
        {
            var blockModelsFromDB = this.GetItems(DXElementDefinitionUnit.ESQLModelDefinition, TypeOfEntityLoading.Full);

            var blockInfos = blockModelsFromDB.Select(x => ESQLObjectHelper.CreateInstance<DXElementDefinitionUnit>(x));

            var blockInfosWithoutCore = blockInfos.Except(CoreDataStructureRepository.CoreBlockInfos, DPObjectDescObjectIDComparer.Instance)
                .Select(x => x as DXElementDefinitionUnit);

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

        public DXRelationDefinitionUnit GetRelation(string objectNameLeft, string relationNameLeft, string objectNameRight, string relationNameRight)
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

        public IEnumerable<DXElementDefinitionUnit> GetBlocks(IEnumerable<Guid> ids)
        {
            if (ids == null)
                return null;

            var result = ids.Select(x => this.GetBlock(x)).Where(x => x != null).ToList();

            return result;
        }

        public DXElementDefinitionUnit GetBlock(Guid id)
        {
            var existingBlock = this.BlockInfos.SingleOrDefault(x => x.ID == id);

            if (existingBlock == null)
            {
                this.UpdateCache();

                existingBlock = this.BlockInfos.SingleOrDefault(x => x.ID == id);
            }

            return existingBlock;
        }

        public IEnumerable<DXEnumDefinitionUnit> GetEnums(IEnumerable<Guid> ids)
        {
            if (ids == null)
                return null;

            var result = ids.Select(x => this.GetEnum(x)).Where(x => x != null).ToList();

            return result;
        }

        public DXEnumDefinitionUnit GetEnum(Guid id)
        {
            var existingEnum = this.EnumInfos.SingleOrDefault(x => x.ID == id);

            if (existingEnum == null)
            {
                this.UpdateCache();

                existingEnum = this.EnumInfos.SingleOrDefault(x => x.ID == id);
            }

            return existingEnum;
        }

        public DXEnumDefinitionUnit GetEnum(string enumName)
        {
            var existingEnum = this.EnumInfos.SingleOrDefault(x => x.DXUnitDefinitionMainElement.Name.Equals(enumName));

            if (existingEnum == null)
            {
                this.UpdateCache();

                existingEnum = this.EnumInfos.SingleOrDefault(x => x.DXUnitDefinitionMainElement.Name.Equals(enumName));
            }

            return existingEnum;
        }

        public void UpdateCache()
        {
            if (MaintenanceToken.IsCoreInitializing)
            {
                this._blockInfos = DXElementDefinitionUnitItems.Items;
                this._entityInfos = DXUnitDefinitionUnitItems.Items;
                this._enumInfos = DXEnumDefinitionUnitItems.Items;
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