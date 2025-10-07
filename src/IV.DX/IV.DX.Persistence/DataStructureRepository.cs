using IV.DX.Kernel.Converters;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using IV.DX.Persistence.CoreData;
using System.Diagnostics.CodeAnalysis;

namespace IV.DX.Persistence
{
    internal partial class DXCoreRepository : IDXCoreRepository, IDXStructureRepository, IDXEnumCoreRepository
    {
        private IList<DXRelationDefinitionUnit> _relationInfos;
        public IEnumerable<DXRelationDefinitionUnit> RelationInfos { get { return this._relationInfos; } }

        private IList<DXUnitDefinitionUnit> _entityInfos;
        public IEnumerable<DXUnitDefinitionUnit> EntityInfos { get { return this._entityInfos; } }

        private IList<DXElementDefinitionUnit> _blockInfos;
        public IEnumerable<DXElementDefinitionUnit> BlockInfos { get { return this._blockInfos; } }

        private IList<DXEnumDefinitionUnit> _enumInfos;
        public IEnumerable<DXEnumDefinitionUnit> EnumInfos { get { return this._enumInfos; } }

        public void CreateDataStructure(DXObjectDefinitionUnit dataBlock)
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

        public void UpdatedDataStructure(DXObjectDefinitionUnit dataBlock)
        {
            var result = this.GetItem(DXModelConverter.GetESQLModelDefinition(typeof(DXObjectDefinitionUnit)), dataBlock.ID, DXLoadingType.Full);
            var existingDataBlock = DXUnitHelper.CreateInstance<DXObjectDefinitionUnit>(result);

            var sqlQuery = this._queryHelper.GetSQLQueryToAlterTable(dataBlock, existingDataBlock);

            this._queryHelper.RunSQLQuery(this._connectionStr, sqlQuery);

            this.UpdateCache();
        }

        public void DropDataStructure(DXObjectDefinitionUnit dataBlock)
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

            switch (obj.DXRelationDefinitionMainElement.RelationType)
            {
                case DXRelationTypeEnum.ManyToMany: result = this._queryHelper.GetSQLQueryToCreateRelationManyToMany(obj, this._connectionStr); break;
                case DXRelationTypeEnum.ManyToOne: result = this.GetSQLQueryToCreateRelationManyToOne(obj); break;
                case DXRelationTypeEnum.ManyToZeroOne: result = this.GetSQLQueryToCreateRelationManyToZeroOne(obj); break;
                case DXRelationTypeEnum.OneToMany: result = this.GetSQLQueryToCreateRelationOneToMany(obj); break;
                case DXRelationTypeEnum.OneToZeroOne: result = this.GetSQLQueryToCreateRelationOneToZeroOne(obj); break;
                case DXRelationTypeEnum.ZeroOneToMany: result = this.GetSQLQueryToCreateRelationZeroOneToMany(obj); break;
                case DXRelationTypeEnum.ZeroOneToOne: result = this.GetSQLQueryToCreateRelationZeroOneToOne(obj); break;
                case DXRelationTypeEnum.ZeroOneToZeroOne: result = this.GetSQLQueryToCreateRelationZeroOneToZeroOne(obj); break;
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
            obj.DXRelationDefinitionMainElement.RelationTable = obj.DXRelationDefinitionMainElement.ObjectNameRight;

            return this._queryHelper.GetSQLQueryToCreateRelationToMany(obj, true, true);
        }

        private string GetSQLQueryToDeleteRelation(DXRelationDefinitionUnit obj)
        {
            string result = "";

            switch (obj.DXRelationDefinitionMainElement.RelationType)
            {
                case DXRelationTypeEnum.ManyToMany: result = this.GetSQLQueryToDeleteRelationManyToMany(obj); break;
                case DXRelationTypeEnum.ManyToOne: result = this._queryHelper.GetSQLQueryToDeleteRelationManyToOne(obj); break;
                case DXRelationTypeEnum.ManyToZeroOne: result = this.GetSQLQueryToDeleteRelationManyToZeroOne(obj); break;
                case DXRelationTypeEnum.OneToMany: result = this._queryHelper.GetSQLQueryToDeleteRelationOneToMany(obj); break;
                case DXRelationTypeEnum.OneToZeroOne: result = this._queryHelper.GetSQLQueryToDeleteRelationOneToZeroOne(obj); break;
                case DXRelationTypeEnum.ZeroOneToMany: result = this.GetSQLQueryToDeleteRelationZeroOneToMany(obj); break;
                case DXRelationTypeEnum.ZeroOneToOne: result = this._queryHelper.GetSQLQueryToDeleteRelationZeroOneToOne(obj); break;
                case DXRelationTypeEnum.ZeroOneToZeroOne: result = this._queryHelper.GetSQLQueryToDeleteRelationOneToZeroOne(obj); break;
            }

            return result;
        }

        private string GetSQLQueryToDeleteRelationManyToMany(DXRelationDefinitionUnit entity)
        {
            string relationTableName;

            if (string.IsNullOrEmpty(entity.DXRelationDefinitionMainElement.RelationTable))
            {
                var existingModel = this.GetItem(DXModelConverter.GetESQLModelDefinition(typeof(DXRelationDefinitionUnit)), entity.ID, DXLoadingType.Full);

                var existingEntity = DXUnitHelper.CreateInstance<DXRelationDefinitionUnit>(existingModel);

                relationTableName = existingEntity.DXRelationDefinitionMainElement.RelationTable;
            }
            else
            {
                relationTableName = entity.DXRelationDefinitionMainElement.RelationTable;
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
            if (derivedEntity == null || derivedEntity.DXUnitInheritanceElement?.BaseEntity == null)
                return null;

            var result = EntityInfos.SingleOrDefault(x => x.ID == derivedEntity.DXUnitInheritanceElement.BaseEntity);

            if (result == null)
            {
                this.UpdateCache();

                result = EntityInfos.SingleOrDefault(x => x.ID == derivedEntity.DXUnitInheritanceElement.BaseEntity);
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
            var enumsModelsFromDB = this.GetItems(DXEnumDefinitionUnit.ESQLModelDefinition, DXLoadingType.Full);

            var enumInfos = enumsModelsFromDB.Select(x => DXUnitHelper.CreateInstance<DXEnumDefinitionUnit>(x));

            var enumInfosWithoutCore = enumInfos.Except(DXCoreDataStructureRepository.CoreEnumInfos, DXObjectDefinitionUnitIDComparer.Instance)
                .Select(x => x as DXEnumDefinitionUnit);

            this._enumInfos = DXCoreDataStructureRepository.CoreEnumInfos.Concat(enumInfosWithoutCore).ToList();
        }

        private void LoadEntityInfos()
        {
            var entityModelsFromDB = this.GetItems(DXUnitDefinitionUnit.ESQLModelDefinition, DXLoadingType.Full);

            var entityInfos = entityModelsFromDB.Select(x => DXUnitHelper.CreateInstance<DXUnitDefinitionUnit>(x));

            var entityInfosWithoutCore = entityInfos.Except(DXCoreDataStructureRepository.CoreEntityInfos, DXObjectDefinitionUnitIDComparer.Instance)
                .Select(x => x as DXUnitDefinitionUnit);

            this._entityInfos = DXCoreDataStructureRepository.CoreEntityInfos.Concat(entityInfosWithoutCore).ToList();
        }

        private void LoadRelationInfos()
        {
            var result = this.GetItems(DXRelationDefinitionUnit.ESQLModelDefinition, DXLoadingType.Full);
            this._relationInfos = result.Select(x => DXUnitHelper.CreateInstance<DXRelationDefinitionUnit>(x)).ToList();
        }

        private void LoadBlockInfos()
        {
            var blockModelsFromDB = this.GetItems(DXElementDefinitionUnit.ESQLModelDefinition, DXLoadingType.Full);

            var blockInfos = blockModelsFromDB.Select(x => DXUnitHelper.CreateInstance<DXElementDefinitionUnit>(x));

            var blockInfosWithoutCore = blockInfos.Except(DXCoreDataStructureRepository.CoreBlockInfos, DXObjectDefinitionUnitIDComparer.Instance)
                .Select(x => x as DXElementDefinitionUnit);

            this._blockInfos = DXCoreDataStructureRepository.CoreBlockInfos.Concat(blockInfosWithoutCore).ToList();
        }

        private class DXObjectDefinitionUnitIDComparer : IEqualityComparer<DXObjectDefinitionUnit>
        {
            public static DXObjectDefinitionUnitIDComparer Instance { get; set; } = new DXObjectDefinitionUnitIDComparer();

            private DXObjectDefinitionUnitIDComparer()
            {

            }

            public bool Equals(DXObjectDefinitionUnit x, DXObjectDefinitionUnit y)
            {
                if (x == null)
                    return false;

                if (y == null)
                    return false;

                if (x.GetType() != y.GetType())
                    return false;

                return x.ID == y.ID;
            }

            public int GetHashCode([DisallowNull] DXObjectDefinitionUnit obj)
            {
                return obj.ID.GetHashCode();
            }
        }

        public DXRelationDefinitionUnit GetRelation(string objectNameLeft, string relationNameLeft, string objectNameRight, string relationNameRight)
        {
            var existingRelation = this.RelationInfos.SingleOrDefault(x =>
                x.DXRelationDefinitionMainElement.ObjectNameLeft == objectNameLeft
                && x.DXRelationDefinitionMainElement.RelationNameLeft == relationNameLeft
                && x.DXRelationDefinitionMainElement.ObjectNameRight == objectNameRight
                && x.DXRelationDefinitionMainElement.RelationNameRight == relationNameRight);

            if (existingRelation == null)
            {
                this.UpdateCache();

                existingRelation = this.RelationInfos.SingleOrDefault(x =>
                x.DXRelationDefinitionMainElement.ObjectNameLeft == objectNameLeft
                && x.DXRelationDefinitionMainElement.RelationNameLeft == relationNameLeft
                && x.DXRelationDefinitionMainElement.ObjectNameRight == objectNameRight
                && x.DXRelationDefinitionMainElement.RelationNameRight == relationNameRight);
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
            if (DXMaintenanceToken.IsCoreInitializing)
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