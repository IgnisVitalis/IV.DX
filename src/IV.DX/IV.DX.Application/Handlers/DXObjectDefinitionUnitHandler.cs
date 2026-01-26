using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel;
using IV.DX.Kernel.Data.Models;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Application.Handlers
{
    internal class DXObjectDefinitionUnitHandler(
        IDXUnitDataService dxUnitService,
        IDXStructureRepository dataStructureRepo,
        IDXUnitGenericRepository genericRepo,
        IDXElementGenericRepository dxElementGenericRepo)
    {
        private string[] systemObjectNames;

        protected string[] SystemObjectNames
        {
            get
            {
                if (systemObjectNames == null)
                {
                    systemObjectNames = GetSystemObjectNames();
                }

                return systemObjectNames;
            }
        }

        private static string[] GetSystemObjectNames()
        {
            var dxElementNames = DXElementDefinitionUnitItems.Items.Select(x => x.Name).ToList();
            var dxUnitNames = DXEnumDefinitionUnitItems.Items.Select(x => x.Name).ToList();
            var dxEnumNames = DXUnitDefinitionUnitItems.Items.Select(x => x.Name).ToList();

            return dxElementNames.Concat(dxUnitNames).Concat(dxEnumNames).ToArray();
        }

        protected void Validate(DXObjectDefinitionUnit dataDXElement)
        {
            if (dataDXElement == null)
                throw new Exception("DXObjectDefinitionUnit is NULL;");

            if (dataDXElement.ID == default(Guid))
                throw new Exception("DXObjectDefinitionUnit.ID has Default value;");

            if (string.IsNullOrEmpty(dataDXElement.Name))
                throw new Exception("DXObjectDefinitionUnit.Name is NULL or Empty;");
        }

        protected void Process(DXObjectDefinitionUnit objectInfoIncome, DXHandlerBaseContext ctx)
        {
            var objectInfoFromDB = GetObjectInfoFromDB<DXObjectDefinitionUnit>(objectInfoIncome, ctx);

            if (objectInfoFromDB == null || objectInfoIncome.DXColumnDefinitionElement.Mode == MultiElementsMode.Full)
            {
                if (objectInfoIncome is DXElementDefinitionUnit)
                {
                    this.SetColumn(objectInfoIncome, objectInfoFromDB, ImportantColumn.DXUnitID);
                }

                this.SetColumn(objectInfoIncome, objectInfoFromDB, ImportantColumn.ID);
                this.SetColumn(objectInfoIncome, objectInfoFromDB, ImportantColumn.TimeStamp);

                this.OrderColumn(objectInfoIncome);
            }
        }

        protected T? GetObjectInfoFromDB<T>(DXObjectDefinitionUnit objectInfoIncome, DXHandlerBaseContext ctx) where T : DXObjectDefinitionUnit
        {
            if (ctx is DXUnitHandlerPreInitCoreContext || ctx is DXUnitHandlerPostInitCoreContext)
            {
                return null;
            }

            return genericRepo.GetDXUnit<T>(objectInfoIncome.ID);
        }

        protected async Task ProcessEnumRelationsAsync(DXObjectDefinitionUnit obj, DXObjectDefinitionUnit? dxUnitExisting, CancellationToken ct)
        {
            if (obj.DXColumnDefinitionElement == null)
                return;

            if (dxUnitExisting == null || obj.DXObjectEnumElement.Mode == MultiElementsMode.Target)
            {
                await ProcessEnumRelationsUsingTargetModeAsync(obj, ct);
            }
            else
            {
                await ProcessEnumRelationsUsingFullModeAsync(obj, dxUnitExisting, ct);
            }
        }

        private async Task ProcessEnumRelationsUsingFullModeAsync(DXObjectDefinitionUnit obj, DXObjectDefinitionUnit dxUnitExisting, CancellationToken ct)
        {
            var currentActualEnumColumns = obj.DXObjectEnumElement.Announced;
            var actualEnumColumns = dxUnitExisting.DXObjectEnumElement.Announced;

            var currentActualEnumColumnIDs = currentActualEnumColumns.Select(x => x.ID).ToList();
            var actualEnumColumnIDs = actualEnumColumns.Select(x => x.ID).ToList();

            var enumColumnIDsToAdd = currentActualEnumColumnIDs.Except(actualEnumColumnIDs);
            var enumColumnIDsToUpdate = currentActualEnumColumnIDs.Intersect(actualEnumColumnIDs);
            var enumColumnIDsToDelete = actualEnumColumnIDs.Except(currentActualEnumColumnIDs);

            foreach (var enumColumnIDToAdd in enumColumnIDsToAdd)
            {
                var enumColumnToAdd = currentActualEnumColumns.Single(x => x.ID == enumColumnIDToAdd);

                var enumInfo = dataStructureRepo.GetDXEnumDefinition(enumColumnToAdd.EnumType);

                var enumColumn = enumInfo.DXColumnDefinitionElement.Announced.Single(x => x.ID == enumColumnToAdd.EnumKey);

                var relationObject = this.CreateDXObjectEnumElementRelationObject(obj, enumInfo, enumColumn, enumColumnToAdd);

                await dxUnitService.InsertAsync(relationObject, new DXUnitHandlerEnumProcessingContext(), ct);
            }

            foreach (var enumColumnIDToUpdate in enumColumnIDsToUpdate)
            {
                var enumColumnToAdd = currentActualEnumColumns.Single(x => x.ID == enumColumnIDToUpdate);

                var enumInfo = dataStructureRepo.GetDXEnumDefinition(enumColumnToAdd.EnumType);

                var enumColumn = enumInfo.DXColumnDefinitionElement.Announced.Single(x => x.ID == enumColumnToAdd.EnumKey);

                var relationObject = this.GetExistingDXObjectEnumElementRelationObject(obj, enumInfo, enumColumn, enumColumnToAdd);

                await dxUnitService.UpdateAsync(relationObject, new DXUnitHandlerEnumProcessingContext(), ct);
            }

            foreach (var enumColumnIDToDelete in enumColumnIDsToDelete)
            {
                var enumColumnToAdd = actualEnumColumns.Single(x => x.ID == enumColumnIDToDelete);

                var enumInfo = dataStructureRepo.GetDXEnumDefinition(enumColumnToAdd.EnumType);

                var enumColumn = enumInfo.DXColumnDefinitionElement.Announced.Single(x => x.ID == enumColumnToAdd.EnumKey);

                var relationObject = this.GetExistingDXObjectEnumElementRelationObject(obj, enumInfo, enumColumn, enumColumnToAdd);

                await dxUnitService.DeleteAsync(relationObject, new DXUnitHandlerEnumProcessingContext(), ct);
            }
        }

        private async Task ProcessEnumRelationsUsingTargetModeAsync(DXObjectDefinitionUnit obj, CancellationToken ct)
        {
            var announcedIds = obj.DXObjectEnumElement.Announced.Select(x => x.EnumType);

            var announcedEnumInfos = dataStructureRepo.GetDXEnumDefinitions(announcedIds);

            var deletedIds = obj.DXObjectEnumElement.Deleted.Select(x => x.EnumType);

            var deletedEnumInfos = dataStructureRepo.GetDXEnumDefinitions(deletedIds);

            foreach (var announcedEnumInfo in announcedEnumInfos)
            {
                var columnWithEnumValue = obj.DXObjectEnumElement.Announced.Single(x => x.EnumType == announcedEnumInfo.ID);

                var enumColumn = announcedEnumInfo.DXColumnDefinitionElement.Announced.Single(x => x.ID == columnWithEnumValue.EnumKey);

                var relationObject = this.CreateDXObjectEnumElementRelationObject(obj, announcedEnumInfo, enumColumn, columnWithEnumValue);

                await dxUnitService.InsertAsync(relationObject, new DXUnitHandlerEnumProcessingContext(), ct);
            }

            foreach (var deletedEnumInfo in deletedEnumInfos)
            {
                var columnWithEnumValue = obj.DXObjectEnumElement.Deleted.Single(x => x.EnumType == deletedEnumInfo.ID);

                var enumColumn = deletedEnumInfo.DXColumnDefinitionElement.Deleted.Single(x => x.ID == columnWithEnumValue.EnumKey);

                var relationObject = this.GetExistingDXObjectEnumElementRelationObject(obj, deletedEnumInfo, enumColumn, columnWithEnumValue);

                await dxUnitService.DeleteAsync(relationObject, new DXUnitHandlerEnumProcessingContext(), ct);
            }
        }

        private DXRelationDefinitionUnit CreateDXObjectEnumElementRelationObject(
            DXObjectDefinitionUnit obj,
            DXEnumDefinitionUnit enumObj,
            DXColumnDefinitionElement enumColumn,
            DXObjectEnumElement columnWithEnumValue)
        {
            var objID = Guid.NewGuid();

            var result = new DXRelationDefinitionUnit()
            {
                ID = objID,
                ObjectNameLeft = obj.Name,
                RelationNameLeft = obj.Name + columnWithEnumValue.Name,
                ObjectNameRight = enumObj.Name,
                RelationNameRight = columnWithEnumValue.Name,
                RelationType = columnWithEnumValue.AllowNull ? DXRelationTypeEnum.ManyToZeroOne : DXRelationTypeEnum.ManyToOne,
                RelationColumnNameRight = enumColumn.Name,
                RelationColumnTypeRight = enumColumn.ColumnType,
                Kind = obj.Kind,
                RelationColumnNameLeft = columnWithEnumValue.Name,
                RelationColumnTypeLeft = enumColumn.ColumnType
            };

            return result;
        }

        private DXRelationDefinitionUnit GetExistingDXObjectEnumElementRelationObject(
           DXObjectDefinitionUnit obj,
           DXEnumDefinitionUnit enumObj,
           DXColumnDefinitionElement enumColumn,
           DXObjectEnumElement columnWithEnumValue)
        {
            string dxFilter =
                $"ObjectNameLeft = '{obj.Name}' " +
                $"AND RelationNameLeft = '{obj.Name + columnWithEnumValue.Name}' " +
                $"AND ObjectNameRight = '{enumObj.Name}' " +
                $"AND RelationColumnNameRight = '{enumColumn.Name}'";

            var existingRelations = genericRepo.GetDXUnits<DXRelationDefinitionUnit>(dxFilter);

            return existingRelations.Single();
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
                    DXUnitID = objectInfoIncome.ID
                };

                this.SetImportantValues(objectIdColumnDesc, column);

                objectInfoIncome.DXColumnDefinitionElement.Announced =
                    objectInfoIncome
                    .DXColumnDefinitionElement
                    .Announced
                    .Append(objectIdColumnDesc).ToHashSet();
            }
            else if (objectIdColumnDescFromDataBase != null && objectIdColumnDescFromModel == null)
            {
                this.SetImportantValues(objectIdColumnDescFromDataBase, column);

                objectInfoIncome.DXColumnDefinitionElement.Announced =
                    objectInfoIncome
                    .DXColumnDefinitionElement
                    .Announced
                    .Append(objectIdColumnDescFromDataBase).ToHashSet();
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

        private DXColumnDefinitionElement GetColumnDesc(DXObjectDefinitionUnit objectInfo, ImportantColumn column)
        {
            string columnName = null;

            switch (column)
            {
                case ImportantColumn.ID:
                    columnName = "id";
                    break;
                case ImportantColumn.DXUnitID:
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
            DXUnitID,
            TimeStamp
        }

        private void SetImportantValues(DXColumnDefinitionElement columnInfo, ImportantColumn columnType)
        {
            switch (columnType)
            {
                case ImportantColumn.ID:
                    this.SetImportantValuesForIDColumn(columnInfo);
                    break;
                case ImportantColumn.DXUnitID:
                    this.SetImportantValuesForDXUnitIDColumn(columnInfo);
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
            idColumn.Name = Constants.ID;
        }

        private void SetImportantValuesForDXUnitIDColumn(DXColumnDefinitionElement objectIDColumn)
        {
            objectIDColumn.AllowNull = false;
            objectIDColumn.DefaultValue = string.Empty;
            objectIDColumn.ColumnType = DXColumnTypeEnum.GUID;
            objectIDColumn.Name = Constants.DXUnitID;
        }

        private void SetImportantValuesForTimeStampColumn(DXColumnDefinitionElement timeStamplColumnDesc)
        {
            timeStamplColumnDesc.AllowNull = false;
            timeStamplColumnDesc.ColumnType = DXColumnTypeEnum.TimeStamp;
            timeStamplColumnDesc.Name = Constants.TimeStamp;
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
                    && x.Name.Trim().ToLower() != "timestamp").ToHashSet();

            // Third
            dataDXElement.DXColumnDefinitionElement.Announced = dataDXElement.DXColumnDefinitionElement.Announced.Prepend(timeStampColumn).ToHashSet();

            // Second
            if (objectIdColumn != null)
            {
                dataDXElement.DXColumnDefinitionElement.Announced = dataDXElement.DXColumnDefinitionElement.Announced.Prepend(objectIdColumn).ToHashSet();
            }

            // First
            dataDXElement.DXColumnDefinitionElement.Announced = dataDXElement.DXColumnDefinitionElement.Announced.Prepend(idColumn).ToHashSet();
        }
    }
}
