using IV.DX.Kernel.Converters;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using System.Diagnostics.CodeAnalysis;

namespace IV.DX.Persistence
{
    internal partial class DXCoreRepository : IDXCoreRepository, IDXStructureRepository, IDXEnumCoreRepository, IDXStructureRawReader
    {
        public void CreateDataStructure(DXObjectDefinitionUnit dataDXElement)
        {
            var sqlQuery = this._queryHelper.GetSQLQueryToCreateTable(dataDXElement);

            this._queryHelper.RunSQLQuery(this._connectionStr, sqlQuery);

            this.UpdateCache();
        }

        public void CreateDataStructure(DXRelationDefinitionUnit dxUnit)
        {
            var sqlQuery = this.GetSQLQueryToCreateRelation(dxUnit);

            this._queryHelper.RunSQLQuery(this._connectionStr, sqlQuery);

            this.UpdateCache();
        }

        public void DropDataStructure(DXRelationDefinitionUnit dxUnit)
        {
            var sqlQuery = this.GetSQLQueryToDeleteRelation(dxUnit);

            this._queryHelper.RunSQLQuery(this._connectionStr, sqlQuery);

            this.UpdateCache();
        }

        public void CreateDataStructure(DXUnitDefinitionUnit obj, DXElementDefinitionUnit dxElement)
        {
            var sqlQuery = this.GetSQLQueryToCreateTable(obj, dxElement);

            this._queryHelper.RunSQLQuery(this._connectionStr, sqlQuery);

            this.UpdateCache();
        }

        public void DropDataStructure(DXUnitDefinitionUnit obj, DXElementDefinitionUnit dxElement)
        {
            var sqlQuery = this.GetSQLQueryToDropTable(obj, dxElement);

            this._queryHelper.RunSQLQuery(this._connectionStr, sqlQuery);

            this.UpdateCache();
        }

        public void UpdatedDataStructure(DXObjectDefinitionUnit dataDXElement)
        {
            var result = this.GetItem(DXModelDefinitionHelper.GetDXModelDefinition(typeof(DXObjectDefinitionUnit)), dataDXElement.ID, DXLoadingType.Full);
            var existingDataDXElement = DXUnitHelper.CreateInstance<DXObjectDefinitionUnit>(result);

            var sqlQuery = this._queryHelper.GetSQLQueryToAlterTable(dataDXElement, existingDataDXElement);

            this._queryHelper.RunSQLQuery(this._connectionStr, sqlQuery);

            this.UpdateCache();
        }

        public void DropDataStructure(DXObjectDefinitionUnit dataDXElement)
        {
            var sqlQuery = this._queryHelper.GetSQLQueryToDropTable(dataDXElement);
            this._queryHelper.RunSQLQuery(this._connectionStr, sqlQuery);

            this.UpdateCache();
        }

        private string GetSQLQueryToDropTable(DXUnitDefinitionUnit obj, DXElementDefinitionUnit dxElement)
        {
            return this._queryHelper.GetSQLQueryToDropTable(obj, dxElement);
        }

        private string GetSQLQueryToCreateTable(DXUnitDefinitionUnit obj, DXElementDefinitionUnit dxElement)
        {
            return this._queryHelper.GetSQLQueryToCreateTable(obj, dxElement);
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

        private string GetSQLQueryToDeleteRelationManyToMany(DXRelationDefinitionUnit dxUnit)
        {
            string relationTableName;

            if (string.IsNullOrEmpty(dxUnit.DXRelationDefinitionMainElement.RelationTable))
            {
                var existingModel = this.GetItem(DXModelDefinitionHelper.GetDXModelDefinition(typeof(DXRelationDefinitionUnit)), dxUnit.ID, DXLoadingType.Full);

                var existingEntity = DXUnitHelper.CreateInstance<DXRelationDefinitionUnit>(existingModel);

                relationTableName = existingEntity.DXRelationDefinitionMainElement.RelationTable;
            }
            else
            {
                relationTableName = dxUnit.DXRelationDefinitionMainElement.RelationTable;
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

        public void SetDXUnitInheritance(string childEntity, string baseEntity)
        {
            var query = this._queryHelper.GetQueryToSetEntityInheritance(childEntity, baseEntity);

            this._queryHelper.RunSQLQuery(this._connectionStr, query);

            this.UpdateCache();
        }

        public DXUnitDefinitionUnit GetBaseDXUnit(DXUnitDefinitionUnit derivedEntity)
        {
            if (derivedEntity == null || derivedEntity.DXUnitInheritanceElement?.BaseEntity == null)
                return null;

            var result = this._dxStructureCache.DXUnits.SingleOrDefault(x => x.ID == derivedEntity.DXUnitInheritanceElement.BaseEntity);

            if (result == null)
            {
                this.UpdateCache();

                result = this._dxStructureCache.DXUnits.SingleOrDefault(x => x.ID == derivedEntity.DXUnitInheritanceElement.BaseEntity);
            }

            return result;
        }

        public DXUnitDefinitionUnit GetDXUnitDefinition(string dxUnitType)
        {
            var result = this._dxStructureCache.DXUnits.SingleOrDefault(x => x.DXUnitDefinitionMainElement.Name.Equals(dxUnitType));

            if (result == null)
            {
                this.UpdateCache();

                result = this._dxStructureCache.DXUnits.SingleOrDefault(x => x.DXUnitDefinitionMainElement.Name.Equals(dxUnitType));
            }

            return result;
        }

        public IEnumerable<DXElementDefinitionUnit> GetRelatedDXElementDefinitions(DXUnitDefinitionUnit dxUnit, DXElementInUnitTypeEnum relationType)
        {
            if (dxUnit.DXElementInUnitDefinitionMainElement == null)
                return null;

            var relatedDXElementIds =
              dxUnit.DXElementInUnitDefinitionMainElement
              .Announced
              .Where(x => x.RelationType == relationType)
              .Select(x => x.DXElementDefinitionUnit).ToList();

            var relatedDXElements = this._dxStructureCache.DXElements.Where(x => relatedDXElementIds.Contains(x.ID)).ToList();

            return relatedDXElements;
        }

        public IEnumerable<DXElementDefinitionUnit> GetRelatedDXElementDefinitions(DXUnitDefinitionUnit dxUnit)
        {
            if (dxUnit.DXElementInUnitDefinitionMainElement == null)
                return null;

            var relatedDXElementIds =
                dxUnit.DXElementInUnitDefinitionMainElement
                .Announced
                .Select(x => x.DXElementDefinitionUnit).ToList();

            var relatedDXElements = this._dxStructureCache.DXElements.Where(x => relatedDXElementIds.Contains(x.ID)).ToList();

            return relatedDXElements;
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

        public DXRelationDefinitionUnit GetDXRelationDefinition(string objectNameLeft, string relationNameLeft, string objectNameRight, string relationNameRight)
        {
            var existingRelation = this._dxStructureCache.DXRelations.SingleOrDefault(x =>
                x.DXRelationDefinitionMainElement.ObjectNameLeft == objectNameLeft
                && x.DXRelationDefinitionMainElement.RelationNameLeft == relationNameLeft
                && x.DXRelationDefinitionMainElement.ObjectNameRight == objectNameRight
                && x.DXRelationDefinitionMainElement.RelationNameRight == relationNameRight);

            if (existingRelation == null)
            {
                this.UpdateCache();

                existingRelation = this._dxStructureCache.DXRelations.SingleOrDefault(x =>
                x.DXRelationDefinitionMainElement.ObjectNameLeft == objectNameLeft
                && x.DXRelationDefinitionMainElement.RelationNameLeft == relationNameLeft
                && x.DXRelationDefinitionMainElement.ObjectNameRight == objectNameRight
                && x.DXRelationDefinitionMainElement.RelationNameRight == relationNameRight);
            }

            return existingRelation;
        }

        public IEnumerable<DXElementDefinitionUnit> GetDXElementDefinitions(IEnumerable<Guid> ids)
        {
            if (ids == null)
                return null;

            var result = ids.Select(x => this.GetDXElementDefinition(x)).Where(x => x != null).ToList();

            return result;
        }

        public DXElementDefinitionUnit GetDXElementDefinition(Guid id)
        {
            var existingDXElement = this._dxStructureCache.DXElements.SingleOrDefault(x => x.ID == id);

            if (existingDXElement == null)
            {
                this.UpdateCache();

                existingDXElement = this._dxStructureCache.DXElements.SingleOrDefault(x => x.ID == id);
            }

            return existingDXElement;
        }

        public IEnumerable<DXEnumDefinitionUnit> GetDXEnumDefinitions(IEnumerable<Guid> ids)
        {
            if (ids == null)
                return null;

            var result = ids.Select(x => this.GetDXEnumDefinition(x)).Where(x => x != null).ToList();

            return result;
        }

        public DXEnumDefinitionUnit GetDXEnumDefinition(Guid id)
        {
            var existingEnum = this._dxStructureCache.DXEnums.SingleOrDefault(x => x.ID == id);

            if (existingEnum == null)
            {
                this.UpdateCache();

                existingEnum = this._dxStructureCache.DXEnums.SingleOrDefault(x => x.ID == id);
            }

            return existingEnum;
        }

        public DXEnumDefinitionUnit GetDXEnumDefinition(string enumName)
        {
            var existingEnum = this._dxStructureCache.DXEnums.SingleOrDefault(x => x.DXUnitDefinitionMainElement.Name.Equals(enumName));

            if (existingEnum == null)
            {
                this.UpdateCache();

                existingEnum = this._dxStructureCache.DXEnums.SingleOrDefault(x => x.DXUnitDefinitionMainElement.Name.Equals(enumName));
            }

            return existingEnum;
        }

        private void UpdateCache()
        {
            this._dxStructureCache.RefreshAsync().Wait();
        }
    }
}