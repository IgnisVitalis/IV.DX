using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace IV.DX.Application.DataHandlers
{
    internal abstract class DPObjectDescObjectHandler<T> : BaseEntityHandler<T> where T : DPObjectDescObject
    {
        private readonly IDataStructureRepository _dataStructureRepo;
        private readonly IDataService _dataService;
        private readonly IGenericRepository _genericRepo;

        protected static readonly string[] systemObjectNames = new[] { "DPObjectDescObject", "DXElementInUnitTypeEnum", "DXUnitDefinitionUnit", "DXElementDefinitionUnit", "DXEnumDefinitionUnit", "DPObjectDescObject", "DPEntityInheritanceBlock", "DXElementInUnitDefinitionMainElement", "DXUnitDefinitionMainElement", "DXColumnDefinitionElement", "DXUniqueColumnsElement", "DXObjectKindEnum", "DXColumnTypeEnum", "DXRelationDefinitionUnit", "DPRelationGenBlock", "DPMigrationScriptsObject", "DPMigrationScriptsGenBlock", "DPRelationTypeEnum" };

        public DPObjectDescObjectHandler(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            this._dataStructureRepo = serviceProvider.GetService<IDataStructureRepository>();
            this._dataService = serviceProvider.GetService<IDataService>();
            this._genericRepo = serviceProvider.GetService<IGenericRepository>();
        }

        protected void ProcessEnumRelations(DPObjectDescObject obj)
        {
            if (obj.DXColumnDefinitionElement == null)
                return;

            // TODO: [70fba884-2cef-4d5f-937d-3efc17365a25]     
            switch (obj.DXColumnDefinitionElement.Mode)
            {
                case ModeForMultiItems.Full:
                    break;
                case ModeForMultiItems.Target:
                    ProcessEnumRelationsUsingTargetMode(obj);
                    break;
                default:
                    break;
            }
        }

        private void ProcessEnumRelationsUsingTargetMode(DPObjectDescObject obj)
        {
            var announcedIds = obj.DXColumnDefinitionElement.Announced.Where(x => x.EnumType.HasValue).Select(x => x.EnumType.Value);

            var announcedEnumInfos = this._dataStructureRepo.GetEnums(announcedIds);

            var deletedIds = obj.DXColumnDefinitionElement.Deleted.Where(x => x.EnumType.HasValue).Select(x => x.EnumType.Value);

            var deletedEnumInfos = this._dataStructureRepo.GetEnums(deletedIds);

            foreach (var announcedEnumInfo in announcedEnumInfos)
            {
                var columnWithEnumValue = obj.DXColumnDefinitionElement.Announced.Single(x => x.EnumType == announcedEnumInfo.ID);

                var enumColumn = announcedEnumInfo.DXColumnDefinitionElement.Announced.Single(x => x.ID == columnWithEnumValue.EnumKey);

                this._dataService.Insert(this.GetRelationObjectForEnum(obj, announcedEnumInfo, enumColumn, columnWithEnumValue));
            }

            foreach (var deletedEnumInfo in deletedEnumInfos)
            {
                var columnWithEnumValue = obj.DXColumnDefinitionElement.Deleted.Single(x => x.EnumType == deletedEnumInfo.ID);

                var enumColumn = deletedEnumInfo.DXColumnDefinitionElement.Deleted.Single(x => x.ID == columnWithEnumValue.EnumKey);

                var relationObject = this.GetRelationObjectForEnum(obj, deletedEnumInfo, enumColumn, columnWithEnumValue);

                this._dataService.Delete("DXRelationDefinitionUnit", relationObject.ID);
            }
        }

        private DXRelationDefinitionUnit GetRelationObjectForEnum(DPObjectDescObject obj, DXEnumDefinitionUnit enumObj, DXColumnDefinitionElement enumColumn, DXColumnDefinitionElement columnWithEnumValue)
        {
            return new DXRelationDefinitionUnit()
            {
                ID = Guid.NewGuid(),
                DPRelationGenBlock = new DPRelationGenBlock()
                {
                    ID = Guid.NewGuid(),
                    ObjectNameLeft = obj.DXUnitDefinitionMainElement.Name,
                    RelationNameLeft = obj.DXUnitDefinitionMainElement.Name,
                    ObjectNameRight = enumObj.DXUnitDefinitionMainElement.Name,
                    RelationNameRight = columnWithEnumValue.Name,
                    RelationType = DPRelationTypeEnum.ManyToOne,
                    RelationColumnNameRight = enumColumn.Name,
                    RelationColumnTypeRight = enumColumn.ColumnType,
                    Kind = obj.DXUnitDefinitionMainElement.Kind
                }
            };
        }

        protected void Validate(DPObjectDescObject dataBlock)
        {
            if (dataBlock == null)
                throw new Exception("DPObjectDescObject is NULL;");

            if (dataBlock.ID == default(Guid))
                throw new Exception("DPObjectDescObject.ID has Default value;");

            if (dataBlock.DXUnitDefinitionMainElement == null)
                throw new Exception("DPObjectDescObject.DXUnitDefinitionMainElement is NULL;");

            if (dataBlock.DXUnitDefinitionMainElement.ID == default(Guid))
                throw new Exception("DPObjectDescObject.DXUnitDefinitionMainElement.ID has Default value;");

            if (string.IsNullOrEmpty(dataBlock.DXUnitDefinitionMainElement.Name))
                throw new Exception("DPObjectDescObject.DXUnitDefinitionMainElement.Name is NULL or Empty;");
        }

        protected void Process(DPObjectDescObject objectInfoIncome)
        {
            var objectInfoFromDB = this.GetObjectInfoFromDB(objectInfoIncome);

            if (objectInfoFromDB == null || objectInfoIncome.DXColumnDefinitionElement.Mode == ModeForMultiItems.Full)
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

        private DPObjectDescObject GetObjectInfoFromDB(DPObjectDescObject objectInfoIncome)
        {
            if (systemObjectNames.Contains(objectInfoIncome.DXUnitDefinitionMainElement.Name, StringComparer.OrdinalIgnoreCase))
                return null;

            return this._genericRepo.GetItem<DPObjectDescObject>(objectInfoIncome.ID);
        }

        private void SetColumn(DPObjectDescObject objectInfoIncome, DPObjectDescObject objectInfoFromDB, ImportantColumn column)
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

        private DXColumnDefinitionElement GetColumnDesc(DPObjectDescObject objectInfo, ImportantColumn column)
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

        private void OrderColumn(DPObjectDescObject dataBlock)
        {
            var idColumn = dataBlock.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.Name.Trim().ToLower() == "id");
            var objectIdColumn = dataBlock.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.Name.Trim().ToLower() == "objectid");
            var timeStampColumn = dataBlock.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.Name.Trim().ToLower() == "timestamp");

            dataBlock.DXColumnDefinitionElement.Announced =
                dataBlock.DXColumnDefinitionElement.Announced
                .Where(x =>
                    x.Name.Trim().ToLower() != "id"
                    && x.Name.Trim().ToLower() != "objectid"
                    && x.Name.Trim().ToLower() != "timestamp");

            // Third
            dataBlock.DXColumnDefinitionElement.Announced = dataBlock.DXColumnDefinitionElement.Announced.Prepend(timeStampColumn);

            // Second
            if (objectIdColumn != null)
            {
                dataBlock.DXColumnDefinitionElement.Announced = dataBlock.DXColumnDefinitionElement.Announced.Prepend(objectIdColumn);
            }

            // First
            dataBlock.DXColumnDefinitionElement.Announced = dataBlock.DXColumnDefinitionElement.Announced.Prepend(idColumn);
        }
    }
}