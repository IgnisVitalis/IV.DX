using IV.DX.Kernel.Converters.DXModelDefinitionConverters;
using IV.DX.Kernel.Converters.DXObjectConverters;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using System.Diagnostics.CodeAnalysis;

namespace IV.DX.Persistence
{
    internal partial class DXCoreRepository : IDXUnitCoreRepository, IDXStructureRepository, IDXEnumCoreRepository, IDXStructureRawReader, IDXElementCoreRepository, IDXRawReader
    {
        public void CreateDataStructure(DXObjectDefinitionUnit dxObjectDefinition)
        {
            var sqlQuery = this._queryHelper.GetSQLQueryToCreateTable(dxObjectDefinition);

            this._queryHelper.RunSQLQuery(this._connectionStr, sqlQuery);
        }

        public void UpdateUniqueColumns(DXObjectDefinitionUnit dxObjectDefinition)
        {
            var sqlQuery = this._queryHelper.GetSQLQueryToSetUniqueColumns(dxObjectDefinition);

            if (!string.IsNullOrEmpty(sqlQuery))
            {
                this._queryHelper.RunSQLQuery(this._connectionStr, sqlQuery);
            }
        }

        public void CreateDataStructure(DXRelationDefinitionUnit dxUnit)
        {
            var sqlQuery = this.GetSQLQueryToCreateRelation(dxUnit);

            this._queryHelper.RunSQLQuery(this._connectionStr, sqlQuery);
        }

        public void DropDataStructure(DXRelationDefinitionUnit dxUnit)
        {
            var sqlQuery = this.GetSQLQueryToDeleteRelation(dxUnit);

            this._queryHelper.RunSQLQuery(this._connectionStr, sqlQuery);
        }

        public void CreateDataStructure(DXUnitDefinitionUnit obj, DXElementDefinitionUnit dxElement)
        {
            var sqlQuery = this.GetSQLQueryToCreateTable(obj, dxElement);

            this._queryHelper.RunSQLQuery(this._connectionStr, sqlQuery);
        }

        public void DropDataStructure(DXUnitDefinitionUnit obj, DXElementDefinitionUnit dxElement)
        {
            var sqlQuery = this.GetSQLQueryToDropTable(obj, dxElement);

            this._queryHelper.RunSQLQuery(this._connectionStr, sqlQuery);
        }

        public void UpdatedDataStructure(DXObjectDefinitionUnit dataDXElement)
        {
            var result = this.GetItem(DXModelDefinitionConverter.ToDXModelDefinition(typeof(DXObjectDefinitionUnit)), dataDXElement.ID, DXLoadingType.Full);
            var existingDataDXElement = DXUnitConverter.ToDXUnits<DXObjectDefinitionUnit>(result);

            var sqlQuery = this._queryHelper.GetSQLQueryToAlterTable(dataDXElement, existingDataDXElement);

            this._queryHelper.RunSQLQuery(this._connectionStr, sqlQuery);
        }

        public void DropDataStructure(DXObjectDefinitionUnit dataDXElement)
        {
            var sqlQuery = this._queryHelper.GetSQLQueryToDropTable(dataDXElement);
            this._queryHelper.RunSQLQuery(this._connectionStr, sqlQuery);
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

            switch (obj.RelationType)
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
            obj.RelationTable = obj.ObjectNameRight;

            return this._queryHelper.GetSQLQueryToCreateRelationToMany(obj, true, true);
        }

        private string GetSQLQueryToDeleteRelation(DXRelationDefinitionUnit obj)
        {
            string result = "";

            switch (obj.RelationType)
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

            if (string.IsNullOrEmpty(dxUnit.RelationTable))
            {
                var existingModel = this.GetItem(DXModelDefinitionConverter.ToDXModelDefinition(typeof(DXRelationDefinitionUnit)), dxUnit.ID, DXLoadingType.Full);

                var existingDXUnit = DXUnitConverter.ToDXUnits<DXRelationDefinitionUnit>(existingModel);

                relationTableName = existingDXUnit.RelationTable;
            }
            else
            {
                relationTableName = dxUnit.RelationTable;
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

        public void SetDXUnitInheritance(string childDXUnit, string baseDXUnit)
        {
            var query = this._queryHelper.GetQueryToSetDXUnitInheritance(childDXUnit, baseDXUnit);

            this._queryHelper.RunSQLQuery(this._connectionStr, query);
        }

        public DXUnitDefinitionUnit? GetBaseDXUnit(DXUnitDefinitionUnit derivedDXUnit)
        {
            if (derivedDXUnit == null || !derivedDXUnit.BaseDXUnit.HasValue)
                return null;

            var result = this._dxStructureCache.DXUnits.SingleOrDefault(x => x.ID == derivedDXUnit.BaseDXUnit);

            if (result == null)
            {
                this.RefreshCache();

                result = this._dxStructureCache.DXUnits.SingleOrDefault(x => x.ID == derivedDXUnit.BaseDXUnit);
            }

            return result;
        }

        public DXUnitDefinitionUnit? GetDXUnitDefinition(string dxUnitType)
        {
            var result = this._dxStructureCache.DXUnits.SingleOrDefault(x => x.Name.Equals(dxUnitType));

            if (result == null)
            {
                this.RefreshCache();

                result = this._dxStructureCache.DXUnits.SingleOrDefault(x => x.Name.Equals(dxUnitType));
            }

            return result;
        }

        public IEnumerable<DXElementDefinitionUnit> GetRelatedDXElementDefinitions(DXUnitDefinitionUnit dxUnit, DXElementInUnitTypeEnum relationType)
        {
            if (dxUnit.DXElementInUnitDefinitionElement == null)
                return null;

            var relatedDXElementIds =
              dxUnit.DXElementInUnitDefinitionElement
              .Announced
              .Where(x => x.RelationType == relationType)
              .Select(x => x.DXElementDefinitionUnit).ToList();

            var relatedDXElements = this._dxStructureCache.DXElements.Where(x => relatedDXElementIds.Contains(x.ID)).ToList();

            return relatedDXElements;
        }

        public IEnumerable<DXElementDefinitionUnit> GetRelatedDXElementDefinitions(DXUnitDefinitionUnit dxUnit)
        {
            if (dxUnit.DXElementInUnitDefinitionElement == null)
                return null;

            var relatedDXElementIds =
                dxUnit.DXElementInUnitDefinitionElement
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
                x.ObjectNameLeft == objectNameLeft
                && x.RelationNameLeft == relationNameLeft
                && x.ObjectNameRight == objectNameRight
                && x.RelationNameRight == relationNameRight);

            if (existingRelation == null)
            {
                this.RefreshCache();

                existingRelation = this._dxStructureCache.DXRelations.SingleOrDefault(x =>
                x.ObjectNameLeft == objectNameLeft
                && x.RelationNameLeft == relationNameLeft
                && x.ObjectNameRight == objectNameRight
                && x.RelationNameRight == relationNameRight);
            }

            return existingRelation;
        }

        public DXUnitDefinitionUnit GetDXUnitDefinition(Guid id)
        {
            var existingDXUnit = this._dxStructureCache.DXUnits.SingleOrDefault(x => x.ID == id);

            if (existingDXUnit == null)
            {
                this.RefreshCache();

                existingDXUnit = this._dxStructureCache.DXUnits.SingleOrDefault(x => x.ID == id);
            }

            return existingDXUnit;
        }

        public IEnumerable<DXUnitDefinitionUnit> GetDXUnitDefinitions(IEnumerable<Guid> ids)
        {
            if (ids == null)
                return null;

            var result = ids.Select(x => this.GetDXUnitDefinition(x)).Where(x => x != null).ToList();

            return result;
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
                this.RefreshCache();

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
                this.RefreshCache();

                existingEnum = this._dxStructureCache.DXEnums.SingleOrDefault(x => x.ID == id);
            }

            return existingEnum;
        }

        public DXEnumDefinitionUnit GetDXEnumDefinition(string enumName)
        {
            var existingEnum = this._dxStructureCache.DXEnums.SingleOrDefault(x => x.Name.Equals(enumName));

            if (existingEnum == null)
            {
                this.RefreshCache();

                existingEnum = this._dxStructureCache.DXEnums.SingleOrDefault(x => x.Name.Equals(enumName));
            }

            return existingEnum;
        }

        private void RefreshCache()
        {
            this._dxStructureCache.RefreshAsync().Wait();
        }
    }
}