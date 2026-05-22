using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel;
using IV.DX.Kernel.Migration.Models;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Application.Handlers
{
    internal class DXObjectDefinitionUnitHandler(
        IDXUnitDataService dxUnitService,
        IDXStructureRepository dataStructureRepo,
        IDXUnitGenericRepository genericRepo)
    {
        private string[]? systemObjectNames;

        protected string[] SystemObjectNames
        {
            get
            {
                if (systemObjectNames == null)
                {
                    systemObjectNames = GetSystemObjectNames();
                }

                return systemObjectNames!;
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

            if (dataDXElement.Id == default(Guid))
                throw new Exception("DXObjectDefinitionUnit.Id has Default value;");

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
                    this.SetColumn(objectInfoIncome, objectInfoFromDB, ImportantColumn.DXUnitId);
                }

                this.SetColumn(objectInfoIncome, objectInfoFromDB, ImportantColumn.Id);
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

            return genericRepo.GetDXUnit<T>(objectInfoIncome.Id);
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

        protected Task ProcessUniqueColumnsAsync(
            DXObjectDefinitionUnit dxObject,
            DXObjectDefinitionUnit? dxObjectOriginal,
            CancellationToken ct)
        {
            NormalizeUniqueColumnsBeforeSave(dxObject, dxObjectOriginal);

            if (dxObjectOriginal == null || dxObject.DXUniqueColumnsElement.Mode == MultiElementsMode.Target)
            {
                this.ProcessDXUniqueColumnsElementsUsingTargetMode(dxObject, dxObjectOriginal);
            }
            else
            {
                this.ProcessDXUniqueColumnsElementsUsingFullMode(dxObject, dxObjectOriginal);
            }
            return Task.CompletedTask;
        }

        // Normalizes DXUniqueColumnsElement.Announced (and Deleted for TargetMode) in-memory before the
        // element records are saved. Must be called in BeforeInsert/BeforeUpdate so the framework persists
        // the clean collection, not the raw user input.
        protected void NormalizeUniqueColumnsBeforeSave(DXObjectDefinitionUnit dxObject, DXObjectDefinitionUnit? dxObjectOriginal)
        {
            var comparer = new StringArraySetComparer(StringComparer.Ordinal);
            var existingElements = dxObjectOriginal?.DXUniqueColumnsElement.Announced
                ?? Enumerable.Empty<DXUniqueColumnsElement>();

            // Deduplicate Announced and replace matching entries with the DB counterpart
            // (preserves element IDs / column ordering from the existing record)
            dxObject.DXUniqueColumnsElement.Announced = DeduplicateUniqueColumns(dxObject.DXUniqueColumnsElement.Announced, comparer)
                .Select(e => MatchToExistingUniqueColumn(e, existingElements, comparer))
                .ToHashSet();

            if (dxObject.DXUniqueColumnsElement.Mode == MultiElementsMode.Target)
            {
                dxObject.DXUniqueColumnsElement.Deleted = DeduplicateUniqueColumns(dxObject.DXUniqueColumnsElement.Deleted, comparer)
                    .Select(e => MatchToExistingUniqueColumn(e, existingElements, comparer))
                    .ToHashSet();
            }
        }

        private void ProcessDXUniqueColumnsElementsUsingFullMode(
          DXObjectDefinitionUnit dxObject,
          DXObjectDefinitionUnit dxObjectOriginal)
        {
            var comparer = new StringArraySetComparer(StringComparer.Ordinal);

            var columnsUniqueAnnounced = this.GetColumnsUnique(dxObject.DXUniqueColumnsElement.Announced);
            var columnsUniqueExisting = this.GetColumnsUnique(dxObjectOriginal.DXUniqueColumnsElement.Announced);

            var onlyInArray1 = columnsUniqueAnnounced.Except(columnsUniqueExisting, comparer).ToArray();
            var onlyInArray2 = columnsUniqueExisting.Except(columnsUniqueAnnounced, comparer).ToArray();

            dataStructureRepo.ProcessUniqueColumns(dxObject.Name, onlyInArray1, onlyInArray2);
        }

        public sealed class StringArraySetComparer : IEqualityComparer<string[]>
        {
            private readonly StringComparer _stringComparer;

            public StringArraySetComparer(StringComparer? stringComparer = null)
            {
                _stringComparer = stringComparer ?? StringComparer.Ordinal;
            }

            public bool Equals(string[]? x, string[]? y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (x is null || y is null) return false;

                return new HashSet<string>(x, _stringComparer).SetEquals(y);
            }

            public int GetHashCode(string[] obj)
            {
                var hash = new HashCode();

                foreach (var item in new HashSet<string>(obj, _stringComparer).OrderBy(x => x, _stringComparer))
                {
                    hash.Add(item, _stringComparer);
                }

                return hash.ToHashCode();
            }
        }

        private void ProcessDXUniqueColumnsElementsUsingTargetMode(
            DXObjectDefinitionUnit dxObject,
            DXObjectDefinitionUnit? dxObjectOriginal)
        {
            var comparer = new StringArraySetComparer(StringComparer.Ordinal);

            var columnsUniqueExisting = dxObjectOriginal != null
                ? this.GetColumnsUnique(dxObjectOriginal.DXUniqueColumnsElement.Announced)
                : Enumerable.Empty<string[]>();

            var columnsUniqueToAdd = this.GetColumnsUnique(dxObject.DXUniqueColumnsElement.Announced)
                .Except(columnsUniqueExisting, comparer);

            var columnsUniqueToRemove = this.GetColumnsUnique(dxObject.DXUniqueColumnsElement.Deleted);

            dataStructureRepo.ProcessUniqueColumns(dxObject.Name, columnsUniqueToAdd, columnsUniqueToRemove);
        }

        private static List<DXUniqueColumnsElement> DeduplicateUniqueColumns(
            IEnumerable<DXUniqueColumnsElement> elements,
            StringArraySetComparer comparer)
        {
            var seen = new HashSet<string[]>(comparer);
            var result = new List<DXUniqueColumnsElement>();
            foreach (var element in elements)
            {
                if (seen.Add(element.Columns.Split(",").Select(y => y.Trim()).ToArray()))
                    result.Add(element);
            }
            return result;
        }

        private static DXUniqueColumnsElement MatchToExistingUniqueColumn(
            DXUniqueColumnsElement element,
            IEnumerable<DXUniqueColumnsElement> existingElements,
            StringArraySetComparer comparer)
        {
            var columns = element.Columns.Split(",").Select(y => y.Trim()).ToArray();
            return existingElements.FirstOrDefault(e =>
                comparer.Equals(e.Columns.Split(",").Select(y => y.Trim()).ToArray(), columns))
                ?? element;
        }

        private IEnumerable<string[]> GetColumnsUnique(IEnumerable<DXUniqueColumnsElement> columsUnique)
        {
            return columsUnique.Select(x => x.Columns.Split(",").Select(y => y.Trim()).ToArray()).ToList();
        }

        private async Task ProcessEnumRelationsUsingFullModeAsync(DXObjectDefinitionUnit obj, DXObjectDefinitionUnit dxUnitExisting, CancellationToken ct)
        {
            var currentActualEnumColumns = obj.DXObjectEnumElement.Announced;
            var actualEnumColumns = dxUnitExisting.DXObjectEnumElement.Announced;

            var currentActualEnumColumnIDs = currentActualEnumColumns.Select(x => x.Id).ToList();
            var actualEnumColumnIDs = actualEnumColumns.Select(x => x.Id).ToList();

            var enumColumnIDsToAdd = currentActualEnumColumnIDs.Except(actualEnumColumnIDs);
            var enumColumnIDsToUpdate = currentActualEnumColumnIDs.Intersect(actualEnumColumnIDs);
            var enumColumnIDsToDelete = actualEnumColumnIDs.Except(currentActualEnumColumnIDs);

            foreach (var enumColumnIDToAdd in enumColumnIDsToAdd)
            {
                var enumColumnToAdd = currentActualEnumColumns.Single(x => x.Id == enumColumnIDToAdd);

                var enumInfo = dataStructureRepo.GetDXEnumDefinition(enumColumnToAdd.EnumType);

                var enumColumn = enumInfo!.DXColumnDefinitionElement.Announced.Single(x => x.Id == enumColumnToAdd.EnumKey);

                var relationObject = this.CreateDXObjectEnumElementRelationObject(obj, enumInfo, enumColumn, enumColumnToAdd);

                await dxUnitService.InsertAsync(relationObject, new DXUnitHandlerEnumProcessingContext(), ct);
            }

            foreach (var enumColumnIDToUpdate in enumColumnIDsToUpdate)
            {
                var enumColumnToAdd = currentActualEnumColumns.Single(x => x.Id == enumColumnIDToUpdate);

                var enumInfo = dataStructureRepo.GetDXEnumDefinition(enumColumnToAdd.EnumType);

                var enumColumn = enumInfo!.DXColumnDefinitionElement.Announced.Single(x => x.Id == enumColumnToAdd.EnumKey);

                var relationObject = this.GetExistingDXObjectEnumElementRelationObject(obj, enumInfo, enumColumn, enumColumnToAdd);

                await dxUnitService.UpdateAsync(relationObject, new DXUnitHandlerEnumProcessingContext(), ct);
            }

            foreach (var enumColumnIDToDelete in enumColumnIDsToDelete)
            {
                var enumColumnToAdd = actualEnumColumns.Single(x => x.Id == enumColumnIDToDelete);

                var enumInfo = dataStructureRepo.GetDXEnumDefinition(enumColumnToAdd.EnumType);

                var enumColumn = enumInfo!.DXColumnDefinitionElement.Announced.Single(x => x.Id == enumColumnToAdd.EnumKey);

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

            foreach (var announcedEnumInfo in announcedEnumInfos!)
            {
                var columnWithEnumValue = obj.DXObjectEnumElement.Announced.Single(x => x.EnumType == announcedEnumInfo.Id);

                var enumColumn = announcedEnumInfo.DXColumnDefinitionElement.Announced.Single(x => x.Id == columnWithEnumValue.EnumKey);

                var relationObject = this.CreateDXObjectEnumElementRelationObject(obj, announcedEnumInfo, enumColumn, columnWithEnumValue);

                await dxUnitService.InsertAsync(relationObject, new DXUnitHandlerEnumProcessingContext(), ct);
            }

            foreach (var deletedEnumInfo in deletedEnumInfos!)
            {
                var columnWithEnumValue = obj.DXObjectEnumElement.Deleted.Single(x => x.EnumType == deletedEnumInfo.Id);

                var enumColumn = deletedEnumInfo.DXColumnDefinitionElement.Deleted.Single(x => x.Id == columnWithEnumValue.EnumKey);

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
            var objId = Guid.CreateVersion7();

            var result = new DXRelationDefinitionUnit()
            {
                Id = objId,
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

        private void SetColumn(DXObjectDefinitionUnit objectInfoIncome, DXObjectDefinitionUnit? objectInfoFromDB, ImportantColumn column)
        {
            var objectIdColumnDescFromModel = this.GetColumnDesc(objectInfoIncome, column);
            var objectIdColumnDescFromDataBase = this.GetColumnDesc(objectInfoFromDB, column);

            if (objectIdColumnDescFromDataBase == null && objectIdColumnDescFromModel == null)
            {
                var objectIdColumnDesc = new DXColumnDefinitionElement()
                {
                    Id = Guid.CreateVersion7(),
                    DXUnitId = objectInfoIncome.Id
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
                objectIdColumnDescFromModel.Id = objectIdColumnDescFromDataBase.Id;

                this.SetImportantValues(objectIdColumnDescFromModel, column);
            }
        }

        private DXColumnDefinitionElement? GetColumnDesc(DXObjectDefinitionUnit? objectInfo, ImportantColumn column)
        {
            string? columnName = null;

            switch (column)
            {
                case ImportantColumn.Id:
                    columnName = "id";
                    break;
                case ImportantColumn.DXUnitId:
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
            Id,
            DXUnitId,
            TimeStamp
        }

        private void SetImportantValues(DXColumnDefinitionElement columnInfo, ImportantColumn columnType)
        {
            switch (columnType)
            {
                case ImportantColumn.Id:
                    this.SetImportantValuesForIDColumn(columnInfo);
                    break;
                case ImportantColumn.DXUnitId:
                    this.SetImportantValuesForDXUnitIdColumn(columnInfo);
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
            idColumn.Name = Constants.Id;
        }

        private void SetImportantValuesForDXUnitIdColumn(DXColumnDefinitionElement objectIDColumn)
        {
            objectIDColumn.AllowNull = false;
            objectIDColumn.DefaultValue = string.Empty;
            objectIDColumn.ColumnType = DXColumnTypeEnum.GUID;
            objectIDColumn.Name = Constants.DXUnitId;
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
            dataDXElement.DXColumnDefinitionElement.Announced = dataDXElement.DXColumnDefinitionElement.Announced.Prepend(timeStampColumn!).ToHashSet();

            // Second
            if (objectIdColumn != null)
            {
                dataDXElement.DXColumnDefinitionElement.Announced = dataDXElement.DXColumnDefinitionElement.Announced.Prepend(objectIdColumn).ToHashSet();
            }

            // First
            dataDXElement.DXColumnDefinitionElement.Announced = dataDXElement.DXColumnDefinitionElement.Announced.Prepend(idColumn!).ToHashSet();
        }
    }
}
