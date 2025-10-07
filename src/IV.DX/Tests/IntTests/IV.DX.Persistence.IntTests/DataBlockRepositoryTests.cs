using IV.DX.Kernel.Converters;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using IV.DX.Shared.IntTests;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace IV.DataProvider.Persistence.Repositories.IntTests
{
    public class DataBlockRepositoryTests : IntTestController
    {
        private readonly TimeSpan difference = new TimeSpan(0, 0, 10);

        IDXGenericRepository _genericRepo;
        
        public DataBlockRepositoryTests(ITestOutputHelper output)
            : base(output)
        {
            this._genericRepo = this.ServiceProvider.GetService<IDXGenericRepository>();
        }

        [Fact]
        public void CreateNewDataBlock_IncludedColumnsWithAllDataTypes_Success()
        {
            // Init
            string json = File.ReadAllText("Assets/JSON/Blocks/DXObjectDefinitionUnit0000.json");
            DXElementDefinitionUnit blockDesc = DXUnitHelper.CreateInstance<DXElementDefinitionUnit>(json);

            base._finalizationAction = new Action(() =>
            {
                this._dataService.Delete(blockDesc);
            });

            void Check(DXElementDefinitionUnit blockDefinition)
            {
                Assert.NotNull(blockDefinition);
                Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), blockDefinition.ID);

                Assert.NotNull(blockDefinition.DXUnitDefinitionMainElement);
                Assert.Equal(new Guid("EB217F3B-1CC3-4CB0-8B5C-E7C71AEDACB4"), blockDefinition.DXUnitDefinitionMainElement.ID);
                Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), blockDefinition.DXUnitDefinitionMainElement.ObjectID);
                Assert.Equal("NewDataBlock", blockDefinition.DXUnitDefinitionMainElement.Name);
                Assert.True(blockDefinition is DXElementDefinitionUnit);

                Assert.NotNull(blockDefinition.DXColumnDefinitionElement);
                Assert.Equal(16, blockDefinition.DXColumnDefinitionElement.Announced.Count());

                var idColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.Name == "ID");
                Assert.NotNull(idColumn);
                Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), idColumn.ObjectID);
                Assert.Equal("ID", idColumn.Name);
                Assert.Equal(DXColumnTypeEnum.GUID, idColumn.ColumnType);

                var objectIdColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.Name == "ObjectID");
                Assert.NotNull(objectIdColumn);
                Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), objectIdColumn.ObjectID);
                Assert.Equal("ObjectID", objectIdColumn.Name);
                Assert.Equal(DXColumnTypeEnum.GUID, objectIdColumn.ColumnType);

                var systemTimeStampColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.Name == "TimeStamp");
                Assert.NotNull(objectIdColumn);
                Assert.Equal(DateTime.UtcNow, systemTimeStampColumn.TimeStamp, difference);
                Assert.Equal("TimeStamp", systemTimeStampColumn.Name);
                Assert.Equal(DXColumnTypeEnum.TimeStamp, systemTimeStampColumn.ColumnType);

                var guidColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("5BCDF497-6004-4028-BB18-5185576E2094"));
                Assert.NotNull(guidColumn);
                Assert.Equal(new Guid("5BCDF497-6004-4028-BB18-5185576E2094"), guidColumn.ID);
                Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), guidColumn.ObjectID);
                Assert.Equal("GuidColumn", guidColumn.Name);
                Assert.Equal(DXColumnTypeEnum.GUID, guidColumn.ColumnType);

                var timeStampColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("C44DB212-4612-4367-8FBF-B5826667EA4C"));
                Assert.NotNull(timeStampColumn);
                Assert.Equal(new Guid("C44DB212-4612-4367-8FBF-B5826667EA4C"), timeStampColumn.ID);
                Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), timeStampColumn.ObjectID);
                Assert.Equal("TimeStampColumn", timeStampColumn.Name);
                Assert.Equal(DXColumnTypeEnum.TimeStamp, timeStampColumn.ColumnType);
                Assert.Equal("CURRENT_TIMESTAMP", timeStampColumn.DefaultValue);

                var stringColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("966EF3FD-B092-4465-9B7C-EBECA6E47CE6"));
                Assert.NotNull(stringColumn);
                Assert.Equal(new Guid("966EF3FD-B092-4465-9B7C-EBECA6E47CE6"), stringColumn.ID);
                Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), stringColumn.ObjectID);
                Assert.Equal("StringColumn", stringColumn.Name);
                Assert.Equal(DXColumnTypeEnum.String, stringColumn.ColumnType);
                Assert.True(stringColumn.AllowNull);
                Assert.Equal(100, stringColumn.Length);
                Assert.Equal("'StringValue'", stringColumn.DefaultValue);

                var textColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("68EBA29A-BFA8-48C4-9FE6-122DE13DA225"));
                Assert.NotNull(textColumn);
                Assert.Equal(new Guid("68EBA29A-BFA8-48C4-9FE6-122DE13DA225"), textColumn.ID);
                Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), textColumn.ObjectID);
                Assert.Equal("TextColumn", textColumn.Name);
                Assert.Equal(DXColumnTypeEnum.Text, textColumn.ColumnType);
                Assert.True(textColumn.AllowNull);

                var dateTimeColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("2A7272C8-73E7-4DBF-B3D1-08BE2C0B47A6"));
                Assert.NotNull(dateTimeColumn);
                Assert.Equal(new Guid("2A7272C8-73E7-4DBF-B3D1-08BE2C0B47A6"), dateTimeColumn.ID);
                Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), dateTimeColumn.ObjectID);
                Assert.Equal("DateTimeColumn", dateTimeColumn.Name);
                Assert.Equal(DXColumnTypeEnum.DateTime, dateTimeColumn.ColumnType);
                Assert.True(dateTimeColumn.AllowNull);
                Assert.Equal("CURRENT_TIMESTAMP", dateTimeColumn.DefaultValue);

                var boolColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("BC13DDCE-1241-4515-BA44-A68320B611A1"));
                Assert.NotNull(boolColumn);
                Assert.Equal(new Guid("BC13DDCE-1241-4515-BA44-A68320B611A1"), boolColumn.ID);
                Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), boolColumn.ObjectID);
                Assert.Equal("BoolColumn", boolColumn.Name);
                Assert.Equal(DXColumnTypeEnum.Bool, boolColumn.ColumnType);
                Assert.True(boolColumn.AllowNull);
                Assert.Equal("0", boolColumn.DefaultValue);

                var shortColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("0C8B0D31-6972-4A93-9355-27E2C873DBAA"));
                Assert.NotNull(shortColumn);
                Assert.Equal(new Guid("0C8B0D31-6972-4A93-9355-27E2C873DBAA"), shortColumn.ID);
                Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), shortColumn.ObjectID);
                Assert.Equal("ShortColumn", shortColumn.Name);
                Assert.Equal(DXColumnTypeEnum.Short, shortColumn.ColumnType);
                Assert.True(shortColumn.AllowNull);
                Assert.Equal("0", shortColumn.DefaultValue);

                var intColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("F505FCAF-7021-46C6-B8AD-8E54C12325B4"));
                Assert.NotNull(intColumn);
                Assert.Equal(new Guid("F505FCAF-7021-46C6-B8AD-8E54C12325B4"), intColumn.ID);
                Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), intColumn.ObjectID);
                Assert.Equal("IntColumn", intColumn.Name);
                Assert.Equal(DXColumnTypeEnum.Int, intColumn.ColumnType);
                Assert.True(intColumn.AllowNull);
                Assert.Equal("0", intColumn.DefaultValue);

                var longColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("5482886C-3062-4F37-B550-41353835C744"));
                Assert.NotNull(longColumn);
                Assert.Equal(new Guid("5482886C-3062-4F37-B550-41353835C744"), longColumn.ID);
                Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), longColumn.ObjectID);
                Assert.Equal("LongColumn", longColumn.Name);
                Assert.Equal(DXColumnTypeEnum.Long, longColumn.ColumnType);
                Assert.True(longColumn.AllowNull);
                Assert.Equal("0", longColumn.DefaultValue);

                var decimalColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("D8C56687-6BC2-4E67-81F1-EFF5F4F2AE89"));
                Assert.NotNull(decimalColumn);
                Assert.Equal(new Guid("D8C56687-6BC2-4E67-81F1-EFF5F4F2AE89"), decimalColumn.ID);
                Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), decimalColumn.ObjectID);
                Assert.Equal("DecimalColumn", decimalColumn.Name);
                Assert.Equal(DXColumnTypeEnum.Decimal, decimalColumn.ColumnType);
                Assert.True(decimalColumn.AllowNull);
                Assert.Equal("0", decimalColumn.DefaultValue);
                Assert.Equal(13, decimalColumn.Precision);
                Assert.Equal(4, decimalColumn.Scale);

                var floatColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("7C8954BF-EEF0-4C18-B240-4FD42D53E27C"));
                Assert.NotNull(floatColumn);
                Assert.Equal(new Guid("7C8954BF-EEF0-4C18-B240-4FD42D53E27C"), floatColumn.ID);
                Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), floatColumn.ObjectID);
                Assert.Equal("FloatColumn", floatColumn.Name);
                Assert.Equal(DXColumnTypeEnum.Float, floatColumn.ColumnType);
                Assert.True(floatColumn.AllowNull);
                Assert.Equal("0", floatColumn.DefaultValue);
                Assert.Equal(8, floatColumn.Precision);
                Assert.Equal(5, floatColumn.Scale);

                var currencyColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("3736BFE6-BF1E-41C3-A72C-A0CA073B1F38"));
                Assert.NotNull(currencyColumn);
                Assert.Equal(new Guid("3736BFE6-BF1E-41C3-A72C-A0CA073B1F38"), currencyColumn.ID);
                Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), currencyColumn.ObjectID);
                Assert.Equal("CurrencyColumn", currencyColumn.Name);
                Assert.Equal(DXColumnTypeEnum.Currency, currencyColumn.ColumnType);
                Assert.True(currencyColumn.AllowNull);
                Assert.Equal("0", currencyColumn.DefaultValue);

                var blobColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("029B39F4-CF55-49D5-876B-6C69C633B856"));
                Assert.NotNull(blobColumn);
                Assert.Equal(new Guid("029B39F4-CF55-49D5-876B-6C69C633B856"), blobColumn.ID);
                Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), blobColumn.ObjectID);
                Assert.Equal("BlobColumn", blobColumn.Name);
                Assert.Equal(DXColumnTypeEnum.Blob, blobColumn.ColumnType);
                Assert.True(blobColumn.AllowNull);
            }

            // Action
            this._dataService.Insert(blockDesc);

            // Checking
            var blockDefinition = this._genericRepo.GetItem<DXElementDefinitionUnit>(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"));

            Check(blockDefinition);

            var blockDefinitions = this._genericRepo.GetItems<DXElementDefinitionUnit>();

            Assert.True(blockDefinitions.Count() > 0);
            blockDefinition = blockDefinitions.SingleOrDefault(x => x.ID == new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"));

            Check(blockDefinition);
        }

        [Fact]
        public void UpdateDataBlock_UpdateAllColumns_Success()
        {
            // Init
            string json0 = File.ReadAllText("Assets/JSON/Blocks/DXObjectDefinitionUnit0000.json");
            string json1 = File.ReadAllText("Assets/JSON/Blocks/DXObjectDefinitionUnit0001.json");

            DXElementDefinitionUnit blockDesc0 = DXUnitHelper.CreateInstance<DXElementDefinitionUnit>(json0);
            DXElementDefinitionUnit blockDesc1 = DXUnitHelper.CreateInstance<DXElementDefinitionUnit>(json1);

            base._finalizationAction = new Action(() =>
            {
                this._dataService.Delete(blockDesc1);
            });

            // Action
            this._dataService.Insert(blockDesc0);
            this._dataService.Update(blockDesc1);

            // Checking
            var blockDefinition = this._genericRepo.GetItem<DXElementDefinitionUnit>(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"));

            Assert.NotNull(blockDefinition);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), blockDefinition.ID);

            Assert.NotNull(blockDefinition.DXUnitDefinitionMainElement);
            Assert.Equal(new Guid("EB217F3B-1CC3-4CB0-8B5C-E7C71AEDACB4"), blockDefinition.DXUnitDefinitionMainElement.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), blockDefinition.DXUnitDefinitionMainElement.ObjectID);
            Assert.Equal("UpdatedDataBlock", blockDefinition.DXUnitDefinitionMainElement.Name);
            Assert.True(blockDefinition is DXElementDefinitionUnit);

            Assert.NotNull(blockDefinition.DXColumnDefinitionElement);
            Assert.Equal(16, blockDefinition.DXColumnDefinitionElement.Announced.Count());

            var idColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.Name == "ID");
            Assert.NotNull(idColumn);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), idColumn.ObjectID);
            Assert.Equal("ID", idColumn.Name);
            Assert.Equal(DXColumnTypeEnum.GUID, idColumn.ColumnType);

            var objectIdColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.Name == "ObjectID");
            Assert.NotNull(objectIdColumn);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), objectIdColumn.ObjectID);
            Assert.Equal("ObjectID", objectIdColumn.Name);
            Assert.Equal(DXColumnTypeEnum.GUID, objectIdColumn.ColumnType);

            var systemTimeStampColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.Name == "TimeStamp");
            Assert.NotNull(systemTimeStampColumn);
            Assert.Equal(DateTime.UtcNow, systemTimeStampColumn.TimeStamp, difference);
            Assert.Equal("TimeStamp", systemTimeStampColumn.Name);
            Assert.Equal(DXColumnTypeEnum.TimeStamp, systemTimeStampColumn.ColumnType);

            var guidColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("5BCDF497-6004-4028-BB18-5185576E2094"));
            Assert.NotNull(guidColumn);
            Assert.Equal(new Guid("5BCDF497-6004-4028-BB18-5185576E2094"), guidColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), guidColumn.ObjectID);
            Assert.Equal("GuidColumn", guidColumn.Name);
            Assert.Equal(DXColumnTypeEnum.GUID, guidColumn.ColumnType);

            var timeStampColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("C44DB212-4612-4367-8FBF-B5826667EA4C"));
            Assert.NotNull(timeStampColumn);
            Assert.Equal(new Guid("C44DB212-4612-4367-8FBF-B5826667EA4C"), timeStampColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), timeStampColumn.ObjectID);
            Assert.Equal("TimeStampColumnUpdated", timeStampColumn.Name);
            Assert.Equal(DXColumnTypeEnum.TimeStamp, timeStampColumn.ColumnType);
            Assert.Equal("CURRENT_TIMESTAMP", timeStampColumn.DefaultValue);

            var stringColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("966EF3FD-B092-4465-9B7C-EBECA6E47CE6"));
            Assert.NotNull(stringColumn);
            Assert.Equal(new Guid("966EF3FD-B092-4465-9B7C-EBECA6E47CE6"), stringColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), stringColumn.ObjectID);
            Assert.Equal("StringColumnUpdated", stringColumn.Name);
            Assert.Equal(DXColumnTypeEnum.String, stringColumn.ColumnType);
            Assert.False(stringColumn.AllowNull);
            Assert.Equal(200, stringColumn.Length);
            Assert.Equal("'StringValueUpdated'", stringColumn.DefaultValue);

            var textColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("68EBA29A-BFA8-48C4-9FE6-122DE13DA225"));
            Assert.NotNull(textColumn);
            Assert.Equal(new Guid("68EBA29A-BFA8-48C4-9FE6-122DE13DA225"), textColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), textColumn.ObjectID);
            Assert.Equal("TextColumnUpdated", textColumn.Name);
            Assert.Equal(DXColumnTypeEnum.Text, textColumn.ColumnType);
            Assert.False(textColumn.AllowNull);

            var dateTimeColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("2A7272C8-73E7-4DBF-B3D1-08BE2C0B47A6"));
            Assert.NotNull(dateTimeColumn);
            Assert.Equal(new Guid("2A7272C8-73E7-4DBF-B3D1-08BE2C0B47A6"), dateTimeColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), dateTimeColumn.ObjectID);
            Assert.Equal("DateTimeColumnUpdated", dateTimeColumn.Name);
            Assert.Equal(DXColumnTypeEnum.DateTime, dateTimeColumn.ColumnType);
            Assert.False(dateTimeColumn.AllowNull);
            Assert.Equal("CURRENT_TIMESTAMP", dateTimeColumn.DefaultValue);

            var boolColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("BC13DDCE-1241-4515-BA44-A68320B611A1"));
            Assert.NotNull(boolColumn);
            Assert.Equal(new Guid("BC13DDCE-1241-4515-BA44-A68320B611A1"), boolColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), boolColumn.ObjectID);
            Assert.Equal("BoolColumnUpdated", boolColumn.Name);
            Assert.Equal(DXColumnTypeEnum.Bool, boolColumn.ColumnType);
            Assert.False(boolColumn.AllowNull);
            Assert.Equal("1", boolColumn.DefaultValue);

            var shortColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("0C8B0D31-6972-4A93-9355-27E2C873DBAA"));
            Assert.NotNull(shortColumn);
            Assert.Equal(new Guid("0C8B0D31-6972-4A93-9355-27E2C873DBAA"), shortColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), shortColumn.ObjectID);
            Assert.Equal("ShortColumnUpdated", shortColumn.Name);
            Assert.Equal(DXColumnTypeEnum.Short, shortColumn.ColumnType);
            Assert.False(shortColumn.AllowNull);
            Assert.Equal("1", shortColumn.DefaultValue);

            var intColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("F505FCAF-7021-46C6-B8AD-8E54C12325B4"));
            Assert.NotNull(intColumn);
            Assert.Equal(new Guid("F505FCAF-7021-46C6-B8AD-8E54C12325B4"), intColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), intColumn.ObjectID);
            Assert.Equal("IntColumnUpdated", intColumn.Name);
            Assert.Equal(DXColumnTypeEnum.Int, intColumn.ColumnType);
            Assert.False(intColumn.AllowNull);
            Assert.Equal("1", intColumn.DefaultValue);

            var longColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("5482886C-3062-4F37-B550-41353835C744"));
            Assert.NotNull(longColumn);
            Assert.Equal(new Guid("5482886C-3062-4F37-B550-41353835C744"), longColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), longColumn.ObjectID);
            Assert.Equal("LongColumnUpdated", longColumn.Name);
            Assert.Equal(DXColumnTypeEnum.Long, longColumn.ColumnType);
            Assert.False(longColumn.AllowNull);
            Assert.Equal("1", longColumn.DefaultValue);

            var decimalColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("D8C56687-6BC2-4E67-81F1-EFF5F4F2AE89"));
            Assert.NotNull(decimalColumn);
            Assert.Equal(new Guid("D8C56687-6BC2-4E67-81F1-EFF5F4F2AE89"), decimalColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), decimalColumn.ObjectID);
            Assert.Equal("DecimalColumnUpdated", decimalColumn.Name);
            Assert.Equal(DXColumnTypeEnum.Decimal, decimalColumn.ColumnType);
            Assert.False(decimalColumn.AllowNull);
            Assert.Equal("1", decimalColumn.DefaultValue);
            Assert.Equal(7, decimalColumn.Precision);
            Assert.Equal(2, decimalColumn.Scale);

            var floatColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("7C8954BF-EEF0-4C18-B240-4FD42D53E27C"));
            Assert.NotNull(floatColumn);
            Assert.Equal(new Guid("7C8954BF-EEF0-4C18-B240-4FD42D53E27C"), floatColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), floatColumn.ObjectID);
            Assert.Equal("FloatColumnUpdated", floatColumn.Name);
            Assert.Equal(DXColumnTypeEnum.Float, floatColumn.ColumnType);
            Assert.False(floatColumn.AllowNull);
            Assert.Equal("1", floatColumn.DefaultValue);
            Assert.Equal(3, floatColumn.Precision);
            Assert.Equal(1, floatColumn.Scale);

            var currencyColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("3736BFE6-BF1E-41C3-A72C-A0CA073B1F38"));
            Assert.NotNull(currencyColumn);
            Assert.Equal(new Guid("3736BFE6-BF1E-41C3-A72C-A0CA073B1F38"), currencyColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), currencyColumn.ObjectID);
            Assert.Equal("CurrencyColumnUpdated", currencyColumn.Name);
            Assert.Equal(DXColumnTypeEnum.Currency, currencyColumn.ColumnType);
            Assert.False(currencyColumn.AllowNull);
            Assert.Equal("1", currencyColumn.DefaultValue);

            var blobColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("029B39F4-CF55-49D5-876B-6C69C633B856"));
            Assert.NotNull(blobColumn);
            Assert.Equal(new Guid("029B39F4-CF55-49D5-876B-6C69C633B856"), blobColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), blobColumn.ObjectID);
            Assert.Equal("BlobColumnUpdated", blobColumn.Name);
            Assert.Equal(DXColumnTypeEnum.Blob, blobColumn.ColumnType);
            Assert.False(blobColumn.AllowNull);
        }

        [Fact]
        public void UpdateDataBlock_RemoveSeveralColumns_Success()
        {
            // Init
            string json0 = File.ReadAllText("Assets/JSON/Blocks/DXObjectDefinitionUnit0000.json");
            string json2 = File.ReadAllText("Assets/JSON/Blocks/DXObjectDefinitionUnit0002.json");
            DXElementDefinitionUnit blockDesc0 = DXUnitHelper.CreateInstance<DXElementDefinitionUnit>(json0);
            DXElementDefinitionUnit blockDesc2 = DXUnitHelper.CreateInstance<DXElementDefinitionUnit>(json2);

            base._finalizationAction = new Action(() =>
            {
                this._dataService.Delete(blockDesc2);
            });

            // Action
            this._dataService.Insert(blockDesc0);
            this._dataService.Update(blockDesc2);

            // Checking
            var blockDefinition = this._genericRepo.GetItem<DXElementDefinitionUnit>(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"));

            Assert.NotNull(blockDefinition);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), blockDefinition.ID);

            Assert.NotNull(blockDefinition.DXUnitDefinitionMainElement);
            Assert.Equal(new Guid("EB217F3B-1CC3-4CB0-8B5C-E7C71AEDACB4"), blockDefinition.DXUnitDefinitionMainElement.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), blockDefinition.DXUnitDefinitionMainElement.ObjectID);
            Assert.Equal("NewDataBlock", blockDefinition.DXUnitDefinitionMainElement.Name);
            Assert.True(blockDefinition is DXElementDefinitionUnit);

            Assert.NotNull(blockDefinition.DXColumnDefinitionElement);
            Assert.Equal(5, blockDefinition.DXColumnDefinitionElement.Announced.Count());

            var idColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.Name == "ID");
            Assert.NotNull(idColumn);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), idColumn.ObjectID);
            Assert.Equal("ID", idColumn.Name);
            Assert.Equal(DXColumnTypeEnum.GUID, idColumn.ColumnType);

            var objectIdColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.Name == "ObjectID");
            Assert.NotNull(objectIdColumn);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), objectIdColumn.ObjectID);
            Assert.Equal("ObjectID", objectIdColumn.Name);
            Assert.Equal(DXColumnTypeEnum.GUID, objectIdColumn.ColumnType);

            var systemTimeStampColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.Name == "TimeStamp");
            Assert.NotNull(systemTimeStampColumn);
            Assert.Equal(DateTime.UtcNow, systemTimeStampColumn.TimeStamp, difference);
            Assert.Equal("TimeStamp", systemTimeStampColumn.Name);
            Assert.Equal(DXColumnTypeEnum.TimeStamp, systemTimeStampColumn.ColumnType);

            var guidColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("5BCDF497-6004-4028-BB18-5185576E2094"));
            Assert.NotNull(guidColumn);
            Assert.Equal(new Guid("5BCDF497-6004-4028-BB18-5185576E2094"), guidColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), guidColumn.ObjectID);
            Assert.Equal("GuidColumn", guidColumn.Name);
            Assert.Equal(DXColumnTypeEnum.GUID, guidColumn.ColumnType);

            var timeStampColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("C44DB212-4612-4367-8FBF-B5826667EA4C"));
            Assert.NotNull(timeStampColumn);
            Assert.Equal(new Guid("C44DB212-4612-4367-8FBF-B5826667EA4C"), timeStampColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), timeStampColumn.ObjectID);
            Assert.Equal("TimeStampColumn", timeStampColumn.Name);
            Assert.Equal(DXColumnTypeEnum.TimeStamp, timeStampColumn.ColumnType);
            Assert.Equal("CURRENT_TIMESTAMP", timeStampColumn.DefaultValue);
        }

        [Fact]
        public void UpdateDataBlock_RemoveAndAddSeveralColumns_Success()
        {
            // Init
            string json0 = File.ReadAllText("Assets/JSON/Blocks/DXObjectDefinitionUnit0000.json");
            string json2 = File.ReadAllText("Assets/JSON/Blocks/DXObjectDefinitionUnit0002.json");
            string json3 = File.ReadAllText("Assets/JSON/Blocks/DXObjectDefinitionUnit0003.json");

            DXElementDefinitionUnit blockDesc0 = DXUnitHelper.CreateInstance<DXElementDefinitionUnit>(json0);
            DXElementDefinitionUnit blockDesc2 = DXUnitHelper.CreateInstance<DXElementDefinitionUnit>(json2);
            DXElementDefinitionUnit blockDesc3 = DXUnitHelper.CreateInstance<DXElementDefinitionUnit>(json3);

            base._finalizationAction = new Action(() =>
            {
                this._dataService.Delete(blockDesc3);
            });

            // Action
            this._dataService.Insert(blockDesc0);
            this._dataService.Update(blockDesc2);
            this._dataService.Update(blockDesc3);

            // Checking
            var blockDefinition = this._genericRepo.GetItem<DXElementDefinitionUnit>(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"));

            Assert.NotNull(blockDefinition);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), blockDefinition.ID);

            Assert.NotNull(blockDefinition.DXUnitDefinitionMainElement);
            Assert.Equal(new Guid("EB217F3B-1CC3-4CB0-8B5C-E7C71AEDACB4"), blockDefinition.DXUnitDefinitionMainElement.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), blockDefinition.DXUnitDefinitionMainElement.ObjectID);
            Assert.Equal("NewDataBlock", blockDefinition.DXUnitDefinitionMainElement.Name);
            Assert.True(blockDefinition is DXElementDefinitionUnit);

            Assert.NotNull(blockDefinition.DXColumnDefinitionElement);
            Assert.Equal(7, blockDefinition.DXColumnDefinitionElement.Announced.Count());

            var idColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.Name == "ID");
            Assert.NotNull(idColumn);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), idColumn.ObjectID);
            Assert.Equal("ID", idColumn.Name);
            Assert.Equal(DXColumnTypeEnum.GUID, idColumn.ColumnType);

            var objectIdColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.Name == "ObjectID");
            Assert.NotNull(objectIdColumn);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), objectIdColumn.ObjectID);
            Assert.Equal("ObjectID", objectIdColumn.Name);
            Assert.Equal(DXColumnTypeEnum.GUID, objectIdColumn.ColumnType);

            var systemTimeStampColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.Name == "TimeStamp");
            Assert.NotNull(systemTimeStampColumn);
            Assert.Equal(DateTime.UtcNow, systemTimeStampColumn.TimeStamp, difference);
            Assert.Equal("TimeStamp", systemTimeStampColumn.Name);
            Assert.Equal(DXColumnTypeEnum.TimeStamp, systemTimeStampColumn.ColumnType);

            var guidColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("5BCDF497-6004-4028-BB18-5185576E2094"));
            Assert.NotNull(guidColumn);
            Assert.Equal(new Guid("5BCDF497-6004-4028-BB18-5185576E2094"), guidColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), guidColumn.ObjectID);
            Assert.Equal("GuidColumn", guidColumn.Name);
            Assert.Equal(DXColumnTypeEnum.GUID, guidColumn.ColumnType);

            var timeStampColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("C44DB212-4612-4367-8FBF-B5826667EA4C"));
            Assert.NotNull(timeStampColumn);
            Assert.Equal(new Guid("C44DB212-4612-4367-8FBF-B5826667EA4C"), timeStampColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), timeStampColumn.ObjectID);
            Assert.Equal("TimeStampColumn", timeStampColumn.Name);
            Assert.Equal(DXColumnTypeEnum.TimeStamp, timeStampColumn.ColumnType);
            Assert.Equal("CURRENT_TIMESTAMP", timeStampColumn.DefaultValue);

            var currencyColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("3736BFE6-BF1E-41C3-A72C-A0CA073B1F38"));
            Assert.NotNull(currencyColumn);
            Assert.Equal(new Guid("3736BFE6-BF1E-41C3-A72C-A0CA073B1F38"), currencyColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), currencyColumn.ObjectID);
            Assert.Equal("CurrencyColumn", currencyColumn.Name);
            Assert.Equal(DXColumnTypeEnum.Currency, currencyColumn.ColumnType);
            Assert.True(currencyColumn.AllowNull);
            Assert.Equal("0", currencyColumn.DefaultValue);

            var blobColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("029B39F4-CF55-49D5-876B-6C69C633B856"));
            Assert.NotNull(blobColumn);
            Assert.Equal(new Guid("029B39F4-CF55-49D5-876B-6C69C633B856"), blobColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), blobColumn.ObjectID);
            Assert.Equal("BlobColumn", blobColumn.Name);
            Assert.Equal(DXColumnTypeEnum.Blob, blobColumn.ColumnType);
            Assert.True(blobColumn.AllowNull);
        }

        [Fact]
        public void UpdateDataBlock_RemoveAndAddAndUpdateSeveralColumns_Success()
        {
            // Init
            string json0 = File.ReadAllText("Assets/JSON/Blocks/DXObjectDefinitionUnit0000.json");
            string json4 = File.ReadAllText("Assets/JSON/Blocks/DXObjectDefinitionUnit0004.json");
            DXElementDefinitionUnit blockDesc0 = DXUnitHelper.CreateInstance<DXElementDefinitionUnit>(json0);
            DXElementDefinitionUnit blockDesc4 = DXUnitHelper.CreateInstance<DXElementDefinitionUnit>(json4);

            base._finalizationAction = new Action(() =>
            {
                this._dataService.Delete(blockDesc4);
            });

            // Action
            this._dataService.Insert(blockDesc0);
            this._dataService.Update(blockDesc4);

            // Checking
            var blockDefinition = this._genericRepo.GetItem<DXElementDefinitionUnit>(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"));

            Assert.NotNull(blockDefinition);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), blockDefinition.ID);

            Assert.NotNull(blockDefinition.DXUnitDefinitionMainElement);
            Assert.Equal(new Guid("EB217F3B-1CC3-4CB0-8B5C-E7C71AEDACB4"), blockDefinition.DXUnitDefinitionMainElement.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), blockDefinition.DXUnitDefinitionMainElement.ObjectID);
            Assert.Equal("UpdatedDataBlock", blockDefinition.DXUnitDefinitionMainElement.Name);
            Assert.True(blockDefinition is DXElementDefinitionUnit);

            Assert.NotNull(blockDefinition.DXColumnDefinitionElement);
            Assert.Equal(8, blockDefinition.DXColumnDefinitionElement.Announced.Count());

            var idColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.Name == "ID");
            Assert.NotNull(idColumn);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), idColumn.ObjectID);
            Assert.Equal("ID", idColumn.Name);
            Assert.Equal(DXColumnTypeEnum.GUID, idColumn.ColumnType);

            var objectIdColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.Name == "ObjectID");
            Assert.NotNull(objectIdColumn);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), objectIdColumn.ObjectID);
            Assert.Equal("ObjectID", objectIdColumn.Name);
            Assert.Equal(DXColumnTypeEnum.GUID, objectIdColumn.ColumnType);

            var systemTimeStampColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.Name == "TimeStamp");
            Assert.NotNull(systemTimeStampColumn);
            Assert.Equal(DateTime.UtcNow, systemTimeStampColumn.TimeStamp, difference);
            Assert.Equal("TimeStamp", systemTimeStampColumn.Name);
            Assert.Equal(DXColumnTypeEnum.TimeStamp, systemTimeStampColumn.ColumnType);

            var guidColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("5BCDF497-6004-4028-BB18-5185576E2094"));
            Assert.NotNull(guidColumn);
            Assert.Equal(new Guid("5BCDF497-6004-4028-BB18-5185576E2094"), guidColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), guidColumn.ObjectID);
            Assert.Equal("GuidColumn", guidColumn.Name);
            Assert.Equal(DXColumnTypeEnum.GUID, guidColumn.ColumnType);

            var timeStampColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("C44DB212-4612-4367-8FBF-B5826667EA4C"));
            Assert.NotNull(timeStampColumn);
            Assert.Equal(new Guid("C44DB212-4612-4367-8FBF-B5826667EA4C"), timeStampColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), timeStampColumn.ObjectID);
            Assert.Equal("TimeStampColumn", timeStampColumn.Name);
            Assert.Equal(DXColumnTypeEnum.TimeStamp, timeStampColumn.ColumnType);
            Assert.Equal("CURRENT_TIMESTAMP", timeStampColumn.DefaultValue);

            var stringColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("E042EF31-397E-4614-BABC-79132D4A68DF"));
            Assert.NotNull(stringColumn);
            Assert.Equal(new Guid("E042EF31-397E-4614-BABC-79132D4A68DF"), stringColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), stringColumn.ObjectID);
            Assert.Equal("StringColumnNew", stringColumn.Name);
            Assert.Equal(DXColumnTypeEnum.String, stringColumn.ColumnType);
            Assert.False(stringColumn.AllowNull);
            Assert.Equal(200, stringColumn.Length);
            Assert.Equal("'StringValueNew'", stringColumn.DefaultValue);

            var currencyColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("3736BFE6-BF1E-41C3-A72C-A0CA073B1F38"));
            Assert.NotNull(currencyColumn);
            Assert.Equal(new Guid("3736BFE6-BF1E-41C3-A72C-A0CA073B1F38"), currencyColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), currencyColumn.ObjectID);
            Assert.Equal("CurrencyColumnUpdated", currencyColumn.Name);
            Assert.Equal(DXColumnTypeEnum.Currency, currencyColumn.ColumnType);
            Assert.False(currencyColumn.AllowNull);
            Assert.Equal("1", currencyColumn.DefaultValue);

            var blobColumn = blockDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("029B39F4-CF55-49D5-876B-6C69C633B856"));
            Assert.NotNull(blobColumn);
            Assert.Equal(new Guid("029B39F4-CF55-49D5-876B-6C69C633B856"), blobColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), blobColumn.ObjectID);
            Assert.Equal("BlobColumnUpdated", blobColumn.Name);
            Assert.Equal(DXColumnTypeEnum.Blob, blobColumn.ColumnType);
            Assert.False(blobColumn.AllowNull);
        }
    }
}