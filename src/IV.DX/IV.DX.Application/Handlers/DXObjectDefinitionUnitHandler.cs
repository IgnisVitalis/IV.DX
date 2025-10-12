using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Application.Handlers
{
    internal class DXObjectDefinitionUnitHandler(IDXUnitDataService dxUnitService, IDXStructureRepository dataStructureRepo, IDXUnitGenericRepository genericRepo)
    {
        protected readonly string[] systemObjectNames = new[] { "DXObjectDefinitionUnit", "DXElementInUnitTypeEnum", "DXUnitDefinitionUnit", "DXElementDefinitionUnit", "DXEnumDefinitionUnit", "DXObjectDefinitionUnit", "DXUnitInheritanceElement", "DXElementInUnitDefinitionMainElement", "DXUnitDefinitionMainElement", "DXColumnDefinitionElement", "DXUniqueColumnsElement", "DXObjectKindEnum", "DXColumnTypeEnum", "DXRelationDefinitionUnit", "DXRelationDefinitionMainElement", "DXMigrationScriptsUnit", "DXMigrationScriptsMainElement", "DXRelationTypeEnum" };

        protected void Validate(DXObjectDefinitionUnit dataDXElement)
        {
            if (dataDXElement == null)
                throw new Exception("DXObjectDefinitionUnit is NULL;");

            if (dataDXElement.ID == default(Guid))
                throw new Exception("DXObjectDefinitionUnit.ID has Default value;");

            if (dataDXElement.DXUnitDefinitionMainElement == null)
                throw new Exception("DXObjectDefinitionUnit.DXUnitDefinitionMainElement is NULL;");

            if (dataDXElement.DXUnitDefinitionMainElement.ID == default(Guid))
                throw new Exception("DXObjectDefinitionUnit.DXUnitDefinitionMainElement.ID has Default value;");

            if (string.IsNullOrEmpty(dataDXElement.DXUnitDefinitionMainElement.Name))
                throw new Exception("DXObjectDefinitionUnit.DXUnitDefinitionMainElement.Name is NULL or Empty;");
        }

        protected void Process(DXObjectDefinitionUnit objectInfoIncome)
        {
            var objectInfoFromDB = this.GetObjectInfoFromDB(objectInfoIncome);

            if (objectInfoFromDB == null || objectInfoIncome.DXColumnDefinitionElement.Mode == MultiElementsMode.Full)
            {
                if (objectInfoIncome is DXElementDefinitionUnit)
                {
                    this.SetColumn(objectInfoIncome, objectInfoFromDB, ImportantColumn.ObjectID);
                }

                this.SetColumn(objectInfoIncome, objectInfoFromDB, ImportantColumn.ID);
                this.SetColumn(objectInfoIncome, objectInfoFromDB, ImportantColumn.TimeStamp);

                this.OrderColumn(objectInfoIncome);
            }
        }

        protected void ProcessEnumRelations(DXObjectDefinitionUnit obj)
        {
            if (obj.DXColumnDefinitionElement == null)
                return;

            // TODO: [70fba884-2cef-4d5f-937d-3efc17365a25]     
            switch (obj.DXColumnDefinitionElement.Mode)
            {
                case MultiElementsMode.Full:
                    break;
                case MultiElementsMode.Target:
                    ProcessEnumRelationsUsingTargetMode(obj);
                    break;
                default:
                    break;
            }
        }

        private void ProcessEnumRelationsUsingTargetMode(DXObjectDefinitionUnit obj)
        {
            var announcedIds = obj.DXColumnDefinitionElement.Announced.Where(x => x.EnumType.HasValue).Select(x => x.EnumType.Value);

            var announcedEnumInfos = dataStructureRepo.GetDXEnumDefinitions(announcedIds);

            var deletedIds = obj.DXColumnDefinitionElement.Deleted.Where(x => x.EnumType.HasValue).Select(x => x.EnumType.Value);

            var deletedEnumInfos = dataStructureRepo.GetDXEnumDefinitions(deletedIds);

            foreach (var announcedEnumInfo in announcedEnumInfos)
            {
                var columnWithEnumValue = obj.DXColumnDefinitionElement.Announced.Single(x => x.EnumType == announcedEnumInfo.ID);

                var enumColumn = announcedEnumInfo.DXColumnDefinitionElement.Announced.Single(x => x.ID == columnWithEnumValue.EnumKey);

                dxUnitService.InsertAsync(this.GetRelationObjectForEnum(obj, announcedEnumInfo, enumColumn, columnWithEnumValue)).Wait();
            }

            foreach (var deletedEnumInfo in deletedEnumInfos)
            {
                var columnWithEnumValue = obj.DXColumnDefinitionElement.Deleted.Single(x => x.EnumType == deletedEnumInfo.ID);

                var enumColumn = deletedEnumInfo.DXColumnDefinitionElement.Deleted.Single(x => x.ID == columnWithEnumValue.EnumKey);

                var relationObject = this.GetRelationObjectForEnum(obj, deletedEnumInfo, enumColumn, columnWithEnumValue);

                dxUnitService.DeleteAsync(relationObject).Wait();
            }
        }

        private DXRelationDefinitionUnit GetRelationObjectForEnum(DXObjectDefinitionUnit obj, DXEnumDefinitionUnit enumObj, DXColumnDefinitionElement enumColumn, DXColumnDefinitionElement columnWithEnumValue)
        {
            return new DXRelationDefinitionUnit()
            {
                ID = Guid.NewGuid(),
                DXRelationDefinitionMainElement = new DXRelationDefinitionMainElement()
                {
                    ID = Guid.NewGuid(),
                    ObjectNameLeft = obj.DXUnitDefinitionMainElement.Name,
                    RelationNameLeft = obj.DXUnitDefinitionMainElement.Name,
                    ObjectNameRight = enumObj.DXUnitDefinitionMainElement.Name,
                    RelationNameRight = columnWithEnumValue.Name,
                    RelationType = DXRelationTypeEnum.ManyToOne,
                    RelationColumnNameRight = enumColumn.Name,
                    RelationColumnTypeRight = enumColumn.ColumnType,
                    Kind = obj.DXUnitDefinitionMainElement.Kind
                }
            };
        }

        private void SetColumn(DXObjectDefinitionUnit objectInfoIncome, DXObjectDefinitionUnit objectInfoFromDB, ImportantColumn column)
        {
            var objectIdColumnDescFromModel = this.GetColumnDesc(objectInfoIncome, column);
            var objectIdColumnDescFromDataBase = this.GetColumnDesc(objectInfoFromDB, column);

            if (objectIdColumnDescFromDataBase == null && objectIdColumnDescFromModel == null)
            {
                var objectIdColumnDesc = new DXColumnDefinitionElement()
                {
                    ID = Guid.NewGuid(),
                    ObjectID = objectInfoIncome.ID
                };

                this.SetImportantValues(objectIdColumnDesc, column);

                objectInfoIncome.DXColumnDefinitionElement.Announced =
                    objectInfoIncome
                    .DXColumnDefinitionElement
                    .Announced
                    .Append(objectIdColumnDesc);
            }
            else if (objectIdColumnDescFromDataBase != null && objectIdColumnDescFromModel == null)
            {
                this.SetImportantValues(objectIdColumnDescFromDataBase, column);

                objectInfoIncome.DXColumnDefinitionElement.Announced =
                    objectInfoIncome
                    .DXColumnDefinitionElement
                    .Announced
                    .Append(objectIdColumnDescFromDataBase);
            }
            else if (objectIdColumnDescFromDataBase == null && objectIdColumnDescFromModel != null)
            {
                this.SetImportantValues(objectIdColumnDescFromModel, column);
            }
            else if (objectIdColumnDescFromDataBase != null && objectIdColumnDescFromModel != null)
            {
                objectIdColumnDescFromModel.ID = objectIdColumnDescFromDataBase.ID;

                this.SetImportantValues(objectIdColumnDescFromModel, column);
            }
        }


        private DXObjectDefinitionUnit GetObjectInfoFromDB(DXObjectDefinitionUnit objectInfoIncome)
        {
            if (systemObjectNames.Contains(objectInfoIncome.DXUnitDefinitionMainElement.Name, StringComparer.OrdinalIgnoreCase))
                return null;

            return genericRepo.GetDXUnit<DXObjectDefinitionUnit>(objectInfoIncome.ID);
        }

        private DXColumnDefinitionElement GetColumnDesc(DXObjectDefinitionUnit objectInfo, ImportantColumn column)
        {
            string columnName = null;

            switch (column)
            {
                case ImportantColumn.ID:
                    columnName = "id";
                    break;
                case ImportantColumn.ObjectID:
                    columnName = "objectid";
                    break;
                case ImportantColumn.TimeStamp:
                    columnName = "timestamp";
                    break;
            }

            return objectInfo?.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.Name.Trim().ToLower() == columnName);
        }

        private enum ImportantColumn
        {
            ID,
            ObjectID,
            TimeStamp
        }

        private void SetImportantValues(DXColumnDefinitionElement columnInfo, ImportantColumn columnType)
        {
            switch (columnType)
            {
                case ImportantColumn.ID:
                    this.SetImportantValuesForIDColumn(columnInfo);
                    break;
                case ImportantColumn.ObjectID:
                    this.SetImportantValuesForObjectIDColumn(columnInfo);
                    break;
                case ImportantColumn.TimeStamp:
                    this.SetImportantValuesForTimeStampColumn(columnInfo);
                    break;
            }
        }

        private void SetImportantValuesForIDColumn(DXColumnDefinitionElement idColumn)
        {
            idColumn.AllowNull = false;
            idColumn.DefaultValue = string.Empty;
            idColumn.ColumnType = DXColumnTypeEnum.GUID;
            idColumn.Name = "ID";
        }

        private void SetImportantValuesForObjectIDColumn(DXColumnDefinitionElement objectIDColumn)
        {
            objectIDColumn.AllowNull = false;
            objectIDColumn.DefaultValue = string.Empty;
            objectIDColumn.ColumnType = DXColumnTypeEnum.GUID;
            objectIDColumn.Name = "ObjectID";
        }

        private void SetImportantValuesForTimeStampColumn(DXColumnDefinitionElement timeStamplColumnDesc)
        {
            timeStamplColumnDesc.AllowNull = false;
            timeStamplColumnDesc.ColumnType = DXColumnTypeEnum.TimeStamp;
            timeStamplColumnDesc.Name = "TimeStamp";
        }

        private void OrderColumn(DXObjectDefinitionUnit dataDXElement)
        {
            var idColumn = dataDXElement.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.Name.Trim().ToLower() == "id");
            var objectIdColumn = dataDXElement.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.Name.Trim().ToLower() == "objectid");
            var timeStampColumn = dataDXElement.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.Name.Trim().ToLower() == "timestamp");

            dataDXElement.DXColumnDefinitionElement.Announced =
                dataDXElement.DXColumnDefinitionElement.Announced
                .Where(x =>
                    x.Name.Trim().ToLower() != "id"
                    && x.Name.Trim().ToLower() != "objectid"
                    && x.Name.Trim().ToLower() != "timestamp");

            // Third
            dataDXElement.DXColumnDefinitionElement.Announced = dataDXElement.DXColumnDefinitionElement.Announced.Prepend(timeStampColumn);

            // Second
            if (objectIdColumn != null)
            {
                dataDXElement.DXColumnDefinitionElement.Announced = dataDXElement.DXColumnDefinitionElement.Announced.Prepend(objectIdColumn);
            }

            // First
            dataDXElement.DXColumnDefinitionElement.Announced = dataDXElement.DXColumnDefinitionElement.Announced.Prepend(idColumn);
        }
    }
}
