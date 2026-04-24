using IV.DX.Kernel.Converters.DXModelDefinitionConverters;
using IV.DX.Kernel.Converters.DXObjectConverters;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using System.Diagnostics.CodeAnalysis;

namespace IV.DX.Persistence
{
    internal partial class DXCoreRepository :
        IDXUnitCoreRepository,
        IDXStructureRepository,
        IDXEnumCoreRepository,
        IDXStructureRawReader,
        IDXElementCoreRepository,
        IDXRawReader
    {
        public void CreateDataStructure(DXObjectDefinitionUnit dxObjectDefinition)
        {
            var sqlQuery = this._schemaHelper.GetSQLQueryToCreateTable(dxObjectDefinition);

            this._dbProvider.RunSQLQuery(this._connectionStr, sqlQuery);
        }

        public void ProcessUniqueColumns(string dxObjectName, IEnumerable<string[]> uniqueColumnsToAdd, IEnumerable<string[]> uniqueColumnsToRemove)
        {
            var sqlQuery = this._schemaHelper.GetSQLQueryToProcessConstraintsForUniqueColumns(dxObjectName, uniqueColumnsToAdd, uniqueColumnsToRemove);

            if (!string.IsNullOrEmpty(sqlQuery))
            {
                this._dbProvider.RunSQLQuery(this._connectionStr, sqlQuery);
            }
        }

        public void CreateDataStructure(DXRelationDefinitionUnit dxUnit)
        {
            var sqlQuery = this.GetSQLQueryToCreateRelation(dxUnit);

            this._dbProvider.RunSQLQuery(this._connectionStr, sqlQuery);
        }

        public void DropDataStructure(DXRelationDefinitionUnit dxUnit)
        {
            var sqlQuery = this.GetSQLQueryToDeleteRelation(dxUnit);

            this._dbProvider.RunSQLQuery(this._connectionStr, sqlQuery);
        }

        public void CreateDataStructure(DXUnitDefinitionUnit obj, DXElementDefinitionUnit dxElement)
        {
            var sqlQuery = this.GetSQLQueryToCreateTable(obj, dxElement);

            this._dbProvider.RunSQLQuery(this._connectionStr, sqlQuery!);
        }

        public void DropDataStructure(DXUnitDefinitionUnit obj, DXElementDefinitionUnit dxElement)
        {
            var sqlQuery = this.GetSQLQueryToDropTable(obj, dxElement);

            this._dbProvider.RunSQLQuery(this._connectionStr, sqlQuery!);
        }

        public void UpdatedDataStructure(DXObjectDefinitionUnit dataDXElement)
        {
            var dxUnitInheritance = _dxStructureCache.GetDXUnitInheritance<DXObjectDefinitionUnit>();

            var block = this.GetItemRecord(
                DXDataSetDefinitionConverter.ToDXModelDefinition(typeof(DXObjectDefinitionUnit), dxUnitInheritance),
                dataDXElement.ID,
                DXLoadingType.Full);
            var record = block?.Data?.Items?.SingleOrDefault();
            var existingDataDXElement = record == null
                ? null
                : (DXObjectDefinitionUnit)DXRecordConverter.ToDXUnit(record, typeof(DXObjectDefinitionUnit));

            var sqlQuery = this._schemaHelper.GetSQLQueryToAlterTable(dataDXElement, existingDataDXElement!);

            this._dbProvider.RunSQLQuery(this._connectionStr, sqlQuery);
        }

        public void DropDataStructure(DXObjectDefinitionUnit dataDXElement)
        {
            var sqlQuery = this._schemaHelper.GetSQLQueryToDropTable(dataDXElement);
            this._dbProvider.RunSQLQuery(this._connectionStr, sqlQuery);
        }

        private string? GetSQLQueryToDropTable(DXUnitDefinitionUnit obj, DXElementDefinitionUnit dxElement)
        {
            return this._schemaHelper.GetSQLQueryToDropTable(obj, dxElement);
        }

        private string? GetSQLQueryToCreateTable(DXUnitDefinitionUnit obj, DXElementDefinitionUnit dxElement)
        {
            return this._schemaHelper.GetSQLQueryToCreateTable(obj, dxElement);
        }

        private string GetSQLQueryToCreateRelation(DXRelationDefinitionUnit obj)
        {
            string result = "";

            switch (obj.RelationType)
            {
                case DXRelationTypeEnum.ManyToMany: result = this._schemaHelper.GetSQLQueryToCreateRelationManyToMany(obj, this._connectionStr); break;
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
            return this._schemaHelper.GetSQLQueryToCreateRelationManyTo(obj, false, false);
        }

        private string GetSQLQueryToCreateRelationManyToZeroOne(DXRelationDefinitionUnit obj)
        {
            return this._schemaHelper.GetSQLQueryToCreateRelationManyTo(obj, true, false);
        }

        private string GetSQLQueryToCreateRelationZeroOneToOne(DXRelationDefinitionUnit obj)
        {
            return this._schemaHelper.GetSQLQueryToCreateRelationManyTo(obj, false, true);
        }

        private string GetSQLQueryToCreateRelationOneToMany(DXRelationDefinitionUnit obj)
        {
            return this._schemaHelper.GetSQLQueryToCreateRelationToMany(obj, false, false);
        }

        private string GetSQLQueryToCreateRelationZeroOneToMany(DXRelationDefinitionUnit obj)
        {
            return this._schemaHelper.GetSQLQueryToCreateRelationToMany(obj, true, false);
        }

        private string GetSQLQueryToCreateRelationOneToZeroOne(DXRelationDefinitionUnit obj)
        {
            return this._schemaHelper.GetSQLQueryToCreateRelationToMany(obj, false, true);
        }

        private string GetSQLQueryToCreateRelationZeroOneToZeroOne(DXRelationDefinitionUnit obj)
        {
            obj.RelationTable = obj.ObjectNameRight;

            return this._schemaHelper.GetSQLQueryToCreateRelationToMany(obj, true, true);
        }

        private string GetSQLQueryToDeleteRelation(DXRelationDefinitionUnit obj)
        {
            string result = "";

            switch (obj.RelationType)
            {
                case DXRelationTypeEnum.ManyToMany: result = this.GetSQLQueryToDeleteRelationManyToMany(obj); break;
                case DXRelationTypeEnum.ManyToOne: result = this._schemaHelper.GetSQLQueryToDeleteRelationManyToOne(obj); break;
                case DXRelationTypeEnum.ManyToZeroOne: result = this.GetSQLQueryToDeleteRelationManyToZeroOne(obj); break;
                case DXRelationTypeEnum.OneToMany: result = this._schemaHelper.GetSQLQueryToDeleteRelationOneToMany(obj); break;
                case DXRelationTypeEnum.OneToZeroOne: result = this._schemaHelper.GetSQLQueryToDeleteRelationOneToZeroOne(obj); break;
                case DXRelationTypeEnum.ZeroOneToMany: result = this.GetSQLQueryToDeleteRelationZeroOneToMany(obj); break;
                case DXRelationTypeEnum.ZeroOneToOne: result = this._schemaHelper.GetSQLQueryToDeleteRelationZeroOneToOne(obj); break;
                case DXRelationTypeEnum.ZeroOneToZeroOne: result = this._schemaHelper.GetSQLQueryToDeleteRelationOneToZeroOne(obj); break;
            }

            return result;
        }

        private string GetSQLQueryToDeleteRelationManyToMany(DXRelationDefinitionUnit dxUnit)
        {
            string? relationTableName;

            if (string.IsNullOrEmpty(dxUnit.RelationTable))
            {
                var dxUnitInheritance = _dxStructureCache.GetDXUnitInheritance<DXRelationDefinitionUnit>();

                var block = this.GetItemRecord(
                    DXDataSetDefinitionConverter.ToDXModelDefinition(typeof(DXRelationDefinitionUnit), dxUnitInheritance),
                    dxUnit.ID,
                    DXLoadingType.Full);
                var record = block?.Data?.Items?.SingleOrDefault();
                var existingDXUnit = record == null
                    ? null
                    : (DXRelationDefinitionUnit)DXRecordConverter.ToDXUnit(record, typeof(DXRelationDefinitionUnit));

                relationTableName = existingDXUnit?.RelationTable;
            }
            else
            {
                relationTableName = dxUnit.RelationTable;
            }

            return this._schemaHelper.GetSQLQueryToDropTable(relationTableName!);
        }

        private string GetSQLQueryToDeleteRelationManyToZeroOne(DXRelationDefinitionUnit obj)
        {
            return this._schemaHelper.GetSQLQueryToDeleteRelationManyToOne(obj);
        }

        private string GetSQLQueryToDeleteRelationZeroOneToMany(DXRelationDefinitionUnit obj)
        {
            return this._schemaHelper.GetSQLQueryToDeleteRelationOneToMany(obj);
        }

        public void SetDXUnitInheritance(string childDXUnit, string baseDXUnit)
        {
            var query = this._schemaHelper.GetQueryToSetDXUnitInheritance(childDXUnit, baseDXUnit);

            this._dbProvider.RunSQLQuery(this._connectionStr, query);
        }

        public void UpdateColumnValue(string tableName, string columnName, object value, Guid id)
        {
            var query = this._schemaHelper.GetSQLQueryToUpdateColumn(tableName, columnName, value, id);

            this._dbProvider.RunSQLQuery(this._connectionStr, query);
        }

        public void UpdateColumnValue(string tableName, string columnName, object value, IDictionary<string, object> whereConditions)
        {
            var query = this._schemaHelper.GetSQLQueryToUpdateColumn(tableName, columnName, value, whereConditions);

            this._dbProvider.RunSQLQuery(this._connectionStr, query);
        }

        public void SetColumnNotNull(string tableName, string columnName)
        {
            var query = this._schemaHelper.GetSQLQueryToSetColumnNotNull(tableName, columnName);

            this._dbProvider.RunSQLQuery(this._connectionStr, query);
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

        private class DXObjectDefinitionUnitIDComparer : IEqualityComparer<DXObjectDefinitionUnit>
        {
            public static DXObjectDefinitionUnitIDComparer Instance { get; set; } = new DXObjectDefinitionUnitIDComparer();

            private DXObjectDefinitionUnitIDComparer()
            {

            }

            public bool Equals(DXObjectDefinitionUnit? x, DXObjectDefinitionUnit? y)
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

        public DXRelationDefinitionUnit? GetDXRelationDefinition(string objectNameLeft, string relationNameLeft, string objectNameRight, string relationNameRight)
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

        public DXUnitDefinitionUnit? GetDXUnitDefinition(Guid id)
        {
            var existingDXUnit = this._dxStructureCache.DXUnits.SingleOrDefault(x => x.ID == id);

            if (existingDXUnit == null)
            {
                this.RefreshCache();

                existingDXUnit = this._dxStructureCache.DXUnits.SingleOrDefault(x => x.ID == id);
            }

            return existingDXUnit;
        }

        public IEnumerable<DXUnitDefinitionUnit>? GetDXUnitDefinitions(IEnumerable<Guid> ids)
        {
            if (ids == null)
                return null;

            var result = ids.Select(x => this.GetDXUnitDefinition(x)).OfType<DXUnitDefinitionUnit>().ToList();

            return result;
        }

        public IEnumerable<DXElementDefinitionUnit>? GetDXElementDefinitions(IEnumerable<Guid> ids)
        {
            if (ids == null)
                return null;

            var result = ids.Select(x => this.GetDXElementDefinition(x)).OfType<DXElementDefinitionUnit>().ToList();

            return result;
        }

        public DXElementDefinitionUnit? GetDXElementDefinition(Guid id)
        {
            var existingDXElement = this._dxStructureCache.DXElements.SingleOrDefault(x => x.ID == id);

            if (existingDXElement == null)
            {
                this.RefreshCache();

                existingDXElement = this._dxStructureCache.DXElements.SingleOrDefault(x => x.ID == id);
            }

            return existingDXElement;
        }

        public IEnumerable<DXEnumDefinitionUnit>? GetDXEnumDefinitions(IEnumerable<Guid> ids)
        {
            if (ids == null)
                return null;

            var result = ids.Select(x => this.GetDXEnumDefinition(x)).OfType<DXEnumDefinitionUnit>().ToList();

            return result;
        }

        public DXEnumDefinitionUnit? GetDXEnumDefinition(Guid id)
        {
            var existingEnum = this._dxStructureCache.DXEnums.SingleOrDefault(x => x.ID == id);

            if (existingEnum == null)
            {
                this.RefreshCache();

                existingEnum = this._dxStructureCache.DXEnums.SingleOrDefault(x => x.ID == id);
            }

            return existingEnum;
        }

        public DXEnumDefinitionUnit? GetDXEnumDefinition(string enumName)
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

