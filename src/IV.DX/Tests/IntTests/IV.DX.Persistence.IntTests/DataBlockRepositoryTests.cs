using IV.DX.Application.Contracts.Abstractions;
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

namespace IV.DX.Persistence.IntTests
{
    [Collection("DX:one-time")]
    public class DataDXElementRepositoryTests : IntTestController
    {
        private readonly TimeSpan difference = new TimeSpan(0, 0, 10);

        IDXUnitGenericRepository _genericRepo;

        IDXUnitDataService _dataService;

        public DataDXElementRepositoryTests(DXTestFixture fx, ITestOutputHelper output)
            : base(fx, output)
        {
            this._genericRepo = this.ServiceProvider.GetRequiredService<IDXUnitGenericRepository>();
            this._dataService = this.ServiceProvider.GetRequiredService<IDXUnitDataService>();
        }

        [Fact]
        public void CreateNewDataDXElement_IncludedColumnsWithAllDataTypes_Success()
        {
            // Init
            string json = File.ReadAllText("Assets/JSON/DXElementDefinitionUnit/DXElementDefinitionUnit0000.json");
            DXElementDefinitionUnit dxElementDesc = DXUnitHelper.CreateInstance<DXElementDefinitionUnit>(json);

            base._finalizationAction = new Action(() =>
            {
                this._dataService.DeleteAsync(dxElementDesc).Wait();
            });

            void Check(DXElementDefinitionUnit dxElementDefinition)
            {
                Assert.NotNull(dxElementDefinition);
                Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), dxElementDefinition.ID);

                Assert.NotNull(dxElementDefinition.DXObjectDefinitionMainElement);
                Assert.Equal(new Guid("EB217F3B-1CC3-4CB0-8B5C-E7C71AEDACB4"), dxElementDefinition.DXObjectDefinitionMainElement.ID);
                Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), dxElementDefinition.DXObjectDefinitionMainElement.ObjectID);
                Assert.Equal("NewDataDXElement", dxElementDefinition.DXObjectDefinitionMainElement.Name);
                Assert.True(dxElementDefinition is DXElementDefinitionUnit);

                Assert.NotNull(dxElementDefinition.DXColumnDefinitionElement);
                Assert.Equal(16, dxElementDefinition.DXColumnDefinitionElement.Announced.Count());

                var idColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.Name == "ID");
                Assert.NotNull(idColumn);
                Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), idColumn.ObjectID);
                Assert.Equal("ID", idColumn.Name);
                Assert.Equal(DXColumnTypeEnum.GUID, idColumn.ColumnType);

                var objectIdColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.Name == "ObjectID");
                Assert.NotNull(objectIdColumn);
                Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), objectIdColumn.ObjectID);
                Assert.Equal("ObjectID", objectIdColumn.Name);
                Assert.Equal(DXColumnTypeEnum.GUID, objectIdColumn.ColumnType);

                var systemTimeStampColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.Name == "TimeStamp");
                Assert.NotNull(objectIdColumn);
                Assert.Equal(DateTime.UtcNow, systemTimeStampColumn.TimeStamp, difference);
                Assert.Equal("TimeStamp", systemTimeStampColumn.Name);
                Assert.Equal(DXColumnTypeEnum.TimeStamp, systemTimeStampColumn.ColumnType);

                var guidColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("5BCDF497-6004-4028-BB18-5185576E2094"));
                Assert.NotNull(guidColumn);
                Assert.Equal(new Guid("5BCDF497-6004-4028-BB18-5185576E2094"), guidColumn.ID);
                Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), guidColumn.ObjectID);
                Assert.Equal("GuidColumn", guidColumn.Name);
                Assert.Equal(DXColumnTypeEnum.GUID, guidColumn.ColumnType);

                var timeStampColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("C44DB212-4612-4367-8FBF-B5826667EA4C"));
                Assert.NotNull(timeStampColumn);
                Assert.Equal(new Guid("C44DB212-4612-4367-8FBF-B5826667EA4C"), timeStampColumn.ID);
                Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), timeStampColumn.ObjectID);
                Assert.Equal("TimeStampColumn", timeStampColumn.Name);
                Assert.Equal(DXColumnTypeEnum.TimeStamp, timeStampColumn.ColumnType);
                Assert.Equal("CURRENT_TIMESTAMP", timeStampColumn.DefaultValue);

                var stringColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("966EF3FD-B092-4465-9B7C-EBECA6E47CE6"));
                Assert.NotNull(stringColumn);
                Assert.Equal(new Guid("966EF3FD-B092-4465-9B7C-EBECA6E47CE6"), stringColumn.ID);
                Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), stringColumn.ObjectID);
                Assert.Equal("StringColumn", stringColumn.Name);
                Assert.Equal(DXColumnTypeEnum.String, stringColumn.ColumnType);
                Assert.True(stringColumn.AllowNull);
                Assert.Equal(100, stringColumn.Length);
                Assert.Equal("'StringValue'", stringColumn.DefaultValue);

                var textColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("68EBA29A-BFA8-48C4-9FE6-122DE13DA225"));
                Assert.NotNull(textColumn);
                Assert.Equal(new Guid("68EBA29A-BFA8-48C4-9FE6-122DE13DA225"), textColumn.ID);
                Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), textColumn.ObjectID);
                Assert.Equal("TextColumn", textColumn.Name);
                Assert.Equal(DXColumnTypeEnum.Text, textColumn.ColumnType);
                Assert.True(textColumn.AllowNull);

                var dateTimeColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("2A7272C8-73E7-4DBF-B3D1-08BE2C0B47A6"));
                Assert.NotNull(dateTimeColumn);
                Assert.Equal(new Guid("2A7272C8-73E7-4DBF-B3D1-08BE2C0B47A6"), dateTimeColumn.ID);
                Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), dateTimeColumn.ObjectID);
                Assert.Equal("DateTimeColumn", dateTimeColumn.Name);
                Assert.Equal(DXColumnTypeEnum.DateTime, dateTimeColumn.ColumnType);
                Assert.True(dateTimeColumn.AllowNull);
                Assert.Equal("CURRENT_TIMESTAMP", dateTimeColumn.DefaultValue);

                var boolColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("BC13DDCE-1241-4515-BA44-A68320B611A1"));
                Assert.NotNull(boolColumn);
                Assert.Equal(new Guid("BC13DDCE-1241-4515-BA44-A68320B611A1"), boolColumn.ID);
                Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), boolColumn.ObjectID);
                Assert.Equal("BoolColumn", boolColumn.Name);
                Assert.Equal(DXColumnTypeEnum.Bool, boolColumn.ColumnType);
                Assert.True(boolColumn.AllowNull);
                Assert.Equal("0", boolColumn.DefaultValue);

                var shortColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("0C8B0D31-6972-4A93-9355-27E2C873DBAA"));
                Assert.NotNull(shortColumn);
                Assert.Equal(new Guid("0C8B0D31-6972-4A93-9355-27E2C873DBAA"), shortColumn.ID);
                Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), shortColumn.ObjectID);
                Assert.Equal("ShortColumn", shortColumn.Name);
                Assert.Equal(DXColumnTypeEnum.Short, shortColumn.ColumnType);
                Assert.True(shortColumn.AllowNull);
                Assert.Equal("0", shortColumn.DefaultValue);

                var intColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("F505FCAF-7021-46C6-B8AD-8E54C12325B4"));
                Assert.NotNull(intColumn);
                Assert.Equal(new Guid("F505FCAF-7021-46C6-B8AD-8E54C12325B4"), intColumn.ID);
                Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), intColumn.ObjectID);
                Assert.Equal("IntColumn", intColumn.Name);
                Assert.Equal(DXColumnTypeEnum.Int, intColumn.ColumnType);
                Assert.True(intColumn.AllowNull);
                Assert.Equal("0", intColumn.DefaultValue);

                var longColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("5482886C-3062-4F37-B550-41353835C744"));
                Assert.NotNull(longColumn);
                Assert.Equal(new Guid("5482886C-3062-4F37-B550-41353835C744"), longColumn.ID);
                Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), longColumn.ObjectID);
                Assert.Equal("LongColumn", longColumn.Name);
                Assert.Equal(DXColumnTypeEnum.Long, longColumn.ColumnType);
                Assert.True(longColumn.AllowNull);
                Assert.Equal("0", longColumn.DefaultValue);

                var decimalColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("D8C56687-6BC2-4E67-81F1-EFF5F4F2AE89"));
                Assert.NotNull(decimalColumn);
                Assert.Equal(new Guid("D8C56687-6BC2-4E67-81F1-EFF5F4F2AE89"), decimalColumn.ID);
                Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), decimalColumn.ObjectID);
                Assert.Equal("DecimalColumn", decimalColumn.Name);
                Assert.Equal(DXColumnTypeEnum.Decimal, decimalColumn.ColumnType);
                Assert.True(decimalColumn.AllowNull);
                Assert.Equal("0", decimalColumn.DefaultValue);
                Assert.Equal(13, decimalColumn.Precision);
                Assert.Equal(4, decimalColumn.Scale);

                var floatColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("7C8954BF-EEF0-4C18-B240-4FD42D53E27C"));
                Assert.NotNull(floatColumn);
                Assert.Equal(new Guid("7C8954BF-EEF0-4C18-B240-4FD42D53E27C"), floatColumn.ID);
                Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), floatColumn.ObjectID);
                Assert.Equal("FloatColumn", floatColumn.Name);
                Assert.Equal(DXColumnTypeEnum.Float, floatColumn.ColumnType);
                Assert.True(floatColumn.AllowNull);
                Assert.Equal("0", floatColumn.DefaultValue);
                Assert.Equal(8, floatColumn.Precision);
                Assert.Equal(5, floatColumn.Scale);

                var currencyColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("3736BFE6-BF1E-41C3-A72C-A0CA073B1F38"));
                Assert.NotNull(currencyColumn);
                Assert.Equal(new Guid("3736BFE6-BF1E-41C3-A72C-A0CA073B1F38"), currencyColumn.ID);
                Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), currencyColumn.ObjectID);
                Assert.Equal("CurrencyColumn", currencyColumn.Name);
                Assert.Equal(DXColumnTypeEnum.Currency, currencyColumn.ColumnType);
                Assert.True(currencyColumn.AllowNull);
                Assert.Equal("0", currencyColumn.DefaultValue);

                var blobColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("029B39F4-CF55-49D5-876B-6C69C633B856"));
                Assert.NotNull(blobColumn);
                Assert.Equal(new Guid("029B39F4-CF55-49D5-876B-6C69C633B856"), blobColumn.ID);
                Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), blobColumn.ObjectID);
                Assert.Equal("BlobColumn", blobColumn.Name);
                Assert.Equal(DXColumnTypeEnum.Blob, blobColumn.ColumnType);
                Assert.True(blobColumn.AllowNull);
            }

            // Action
            this._dataService.InsertAsync(dxElementDesc).Wait();

            // Checking
            var dxElementDefinition = this._genericRepo.GetDXUnit<DXElementDefinitionUnit>(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"));

            Check(dxElementDefinition);

            var dxElementDefinitions = this._genericRepo.GetDXUnits<DXElementDefinitionUnit>();

            Assert.True(dxElementDefinitions.Count() > 0);
            dxElementDefinition = dxElementDefinitions.SingleOrDefault(x => x.ID == new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"));

            Check(dxElementDefinition);
        }

        [Fact]
        public void UpdateDataDXElement_UpdateAllColumns_Success()
        {
            // Init
            string json0 = File.ReadAllText("Assets/JSON/DXElementDefinitionUnit/DXElementDefinitionUnit0000.json");
            string json1 = File.ReadAllText("Assets/JSON/DXElementDefinitionUnit/DXElementDefinitionUnit0001.json");

            DXElementDefinitionUnit dxElementDesc0 = DXUnitHelper.CreateInstance<DXElementDefinitionUnit>(json0);
            DXElementDefinitionUnit dxElementDesc1 = DXUnitHelper.CreateInstance<DXElementDefinitionUnit>(json1);

            base._finalizationAction = new Action(() =>
            {
                this._dataService.DeleteAsync(dxElementDesc1).Wait();
            });

            // Action
            this._dataService.InsertAsync(dxElementDesc0).Wait();
            this._dataService.UpdateAsync(dxElementDesc1).Wait();

            // Checking
            var dxElementDefinition = this._genericRepo.GetDXUnit<DXElementDefinitionUnit>(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"));

            Assert.NotNull(dxElementDefinition);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), dxElementDefinition.ID);

            Assert.NotNull(dxElementDefinition.DXObjectDefinitionMainElement);
            Assert.Equal(new Guid("EB217F3B-1CC3-4CB0-8B5C-E7C71AEDACB4"), dxElementDefinition.DXObjectDefinitionMainElement.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), dxElementDefinition.DXObjectDefinitionMainElement.ObjectID);
            Assert.Equal("UpdatedDataDXElement", dxElementDefinition.DXObjectDefinitionMainElement.Name);
            Assert.True(dxElementDefinition is DXElementDefinitionUnit);

            Assert.NotNull(dxElementDefinition.DXColumnDefinitionElement);
            Assert.Equal(16, dxElementDefinition.DXColumnDefinitionElement.Announced.Count());

            var idColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.Name == "ID");
            Assert.NotNull(idColumn);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), idColumn.ObjectID);
            Assert.Equal("ID", idColumn.Name);
            Assert.Equal(DXColumnTypeEnum.GUID, idColumn.ColumnType);

            var objectIdColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.Name == "ObjectID");
            Assert.NotNull(objectIdColumn);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), objectIdColumn.ObjectID);
            Assert.Equal("ObjectID", objectIdColumn.Name);
            Assert.Equal(DXColumnTypeEnum.GUID, objectIdColumn.ColumnType);

            var systemTimeStampColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.Name == "TimeStamp");
            Assert.NotNull(systemTimeStampColumn);
            Assert.Equal(DateTime.UtcNow, systemTimeStampColumn.TimeStamp, difference);
            Assert.Equal("TimeStamp", systemTimeStampColumn.Name);
            Assert.Equal(DXColumnTypeEnum.TimeStamp, systemTimeStampColumn.ColumnType);

            var guidColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("5BCDF497-6004-4028-BB18-5185576E2094"));
            Assert.NotNull(guidColumn);
            Assert.Equal(new Guid("5BCDF497-6004-4028-BB18-5185576E2094"), guidColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), guidColumn.ObjectID);
            Assert.Equal("GuidColumn", guidColumn.Name);
            Assert.Equal(DXColumnTypeEnum.GUID, guidColumn.ColumnType);

            var timeStampColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("C44DB212-4612-4367-8FBF-B5826667EA4C"));
            Assert.NotNull(timeStampColumn);
            Assert.Equal(new Guid("C44DB212-4612-4367-8FBF-B5826667EA4C"), timeStampColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), timeStampColumn.ObjectID);
            Assert.Equal("TimeStampColumnUpdated", timeStampColumn.Name);
            Assert.Equal(DXColumnTypeEnum.TimeStamp, timeStampColumn.ColumnType);
            Assert.Equal("CURRENT_TIMESTAMP", timeStampColumn.DefaultValue);

            var stringColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("966EF3FD-B092-4465-9B7C-EBECA6E47CE6"));
            Assert.NotNull(stringColumn);
            Assert.Equal(new Guid("966EF3FD-B092-4465-9B7C-EBECA6E47CE6"), stringColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), stringColumn.ObjectID);
            Assert.Equal("StringColumnUpdated", stringColumn.Name);
            Assert.Equal(DXColumnTypeEnum.String, stringColumn.ColumnType);
            Assert.False(stringColumn.AllowNull);
            Assert.Equal(200, stringColumn.Length);
            Assert.Equal("'StringValueUpdated'", stringColumn.DefaultValue);

            var textColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("68EBA29A-BFA8-48C4-9FE6-122DE13DA225"));
            Assert.NotNull(textColumn);
            Assert.Equal(new Guid("68EBA29A-BFA8-48C4-9FE6-122DE13DA225"), textColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), textColumn.ObjectID);
            Assert.Equal("TextColumnUpdated", textColumn.Name);
            Assert.Equal(DXColumnTypeEnum.Text, textColumn.ColumnType);
            Assert.False(textColumn.AllowNull);

            var dateTimeColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("2A7272C8-73E7-4DBF-B3D1-08BE2C0B47A6"));
            Assert.NotNull(dateTimeColumn);
            Assert.Equal(new Guid("2A7272C8-73E7-4DBF-B3D1-08BE2C0B47A6"), dateTimeColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), dateTimeColumn.ObjectID);
            Assert.Equal("DateTimeColumnUpdated", dateTimeColumn.Name);
            Assert.Equal(DXColumnTypeEnum.DateTime, dateTimeColumn.ColumnType);
            Assert.False(dateTimeColumn.AllowNull);
            Assert.Equal("CURRENT_TIMESTAMP", dateTimeColumn.DefaultValue);

            var boolColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("BC13DDCE-1241-4515-BA44-A68320B611A1"));
            Assert.NotNull(boolColumn);
            Assert.Equal(new Guid("BC13DDCE-1241-4515-BA44-A68320B611A1"), boolColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), boolColumn.ObjectID);
            Assert.Equal("BoolColumnUpdated", boolColumn.Name);
            Assert.Equal(DXColumnTypeEnum.Bool, boolColumn.ColumnType);
            Assert.False(boolColumn.AllowNull);
            Assert.Equal("1", boolColumn.DefaultValue);

            var shortColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("0C8B0D31-6972-4A93-9355-27E2C873DBAA"));
            Assert.NotNull(shortColumn);
            Assert.Equal(new Guid("0C8B0D31-6972-4A93-9355-27E2C873DBAA"), shortColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), shortColumn.ObjectID);
            Assert.Equal("ShortColumnUpdated", shortColumn.Name);
            Assert.Equal(DXColumnTypeEnum.Short, shortColumn.ColumnType);
            Assert.False(shortColumn.AllowNull);
            Assert.Equal("1", shortColumn.DefaultValue);

            var intColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("F505FCAF-7021-46C6-B8AD-8E54C12325B4"));
            Assert.NotNull(intColumn);
            Assert.Equal(new Guid("F505FCAF-7021-46C6-B8AD-8E54C12325B4"), intColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), intColumn.ObjectID);
            Assert.Equal("IntColumnUpdated", intColumn.Name);
            Assert.Equal(DXColumnTypeEnum.Int, intColumn.ColumnType);
            Assert.False(intColumn.AllowNull);
            Assert.Equal("1", intColumn.DefaultValue);

            var longColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("5482886C-3062-4F37-B550-41353835C744"));
            Assert.NotNull(longColumn);
            Assert.Equal(new Guid("5482886C-3062-4F37-B550-41353835C744"), longColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), longColumn.ObjectID);
            Assert.Equal("LongColumnUpdated", longColumn.Name);
            Assert.Equal(DXColumnTypeEnum.Long, longColumn.ColumnType);
            Assert.False(longColumn.AllowNull);
            Assert.Equal("1", longColumn.DefaultValue);

            var decimalColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("D8C56687-6BC2-4E67-81F1-EFF5F4F2AE89"));
            Assert.NotNull(decimalColumn);
            Assert.Equal(new Guid("D8C56687-6BC2-4E67-81F1-EFF5F4F2AE89"), decimalColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), decimalColumn.ObjectID);
            Assert.Equal("DecimalColumnUpdated", decimalColumn.Name);
            Assert.Equal(DXColumnTypeEnum.Decimal, decimalColumn.ColumnType);
            Assert.False(decimalColumn.AllowNull);
            Assert.Equal("1", decimalColumn.DefaultValue);
            Assert.Equal(7, decimalColumn.Precision);
            Assert.Equal(2, decimalColumn.Scale);

            var floatColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("7C8954BF-EEF0-4C18-B240-4FD42D53E27C"));
            Assert.NotNull(floatColumn);
            Assert.Equal(new Guid("7C8954BF-EEF0-4C18-B240-4FD42D53E27C"), floatColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), floatColumn.ObjectID);
            Assert.Equal("FloatColumnUpdated", floatColumn.Name);
            Assert.Equal(DXColumnTypeEnum.Float, floatColumn.ColumnType);
            Assert.False(floatColumn.AllowNull);
            Assert.Equal("1", floatColumn.DefaultValue);
            Assert.Equal(3, floatColumn.Precision);
            Assert.Equal(1, floatColumn.Scale);

            var currencyColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("3736BFE6-BF1E-41C3-A72C-A0CA073B1F38"));
            Assert.NotNull(currencyColumn);
            Assert.Equal(new Guid("3736BFE6-BF1E-41C3-A72C-A0CA073B1F38"), currencyColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), currencyColumn.ObjectID);
            Assert.Equal("CurrencyColumnUpdated", currencyColumn.Name);
            Assert.Equal(DXColumnTypeEnum.Currency, currencyColumn.ColumnType);
            Assert.False(currencyColumn.AllowNull);
            Assert.Equal("1", currencyColumn.DefaultValue);

            var blobColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("029B39F4-CF55-49D5-876B-6C69C633B856"));
            Assert.NotNull(blobColumn);
            Assert.Equal(new Guid("029B39F4-CF55-49D5-876B-6C69C633B856"), blobColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), blobColumn.ObjectID);
            Assert.Equal("BlobColumnUpdated", blobColumn.Name);
            Assert.Equal(DXColumnTypeEnum.Blob, blobColumn.ColumnType);
            Assert.False(blobColumn.AllowNull);
        }

        [Fact]
        public void UpdateDataDXElement_RemoveSeveralColumns_Success()
        {
            // Init
            string json0 = File.ReadAllText("Assets/JSON/DXElementDefinitionUnit/DXElementDefinitionUnit0000.json");
            string json2 = File.ReadAllText("Assets/JSON/DXElementDefinitionUnit/DXElementDefinitionUnit0002.json");
            DXElementDefinitionUnit dxElementDesc0 = DXUnitHelper.CreateInstance<DXElementDefinitionUnit>(json0);
            DXElementDefinitionUnit dxElementDesc2 = DXUnitHelper.CreateInstance<DXElementDefinitionUnit>(json2);

            base._finalizationAction = new Action(() =>
            {
                this._dataService.DeleteAsync(dxElementDesc2).Wait();
            });

            // Action
            this._dataService.InsertAsync(dxElementDesc0).Wait();
            this._dataService.UpdateAsync(dxElementDesc2).Wait();

            // Checking
            var dxElementDefinition = this._genericRepo.GetDXUnit<DXElementDefinitionUnit>(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"));

            Assert.NotNull(dxElementDefinition);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), dxElementDefinition.ID);

            Assert.NotNull(dxElementDefinition.DXObjectDefinitionMainElement);
            Assert.Equal(new Guid("EB217F3B-1CC3-4CB0-8B5C-E7C71AEDACB4"), dxElementDefinition.DXObjectDefinitionMainElement.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), dxElementDefinition.DXObjectDefinitionMainElement.ObjectID);
            Assert.Equal("NewDataDXElement", dxElementDefinition.DXObjectDefinitionMainElement.Name);
            Assert.True(dxElementDefinition is DXElementDefinitionUnit);

            Assert.NotNull(dxElementDefinition.DXColumnDefinitionElement);
            Assert.Equal(5, dxElementDefinition.DXColumnDefinitionElement.Announced.Count());

            var idColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.Name == "ID");
            Assert.NotNull(idColumn);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), idColumn.ObjectID);
            Assert.Equal("ID", idColumn.Name);
            Assert.Equal(DXColumnTypeEnum.GUID, idColumn.ColumnType);

            var objectIdColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.Name == "ObjectID");
            Assert.NotNull(objectIdColumn);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), objectIdColumn.ObjectID);
            Assert.Equal("ObjectID", objectIdColumn.Name);
            Assert.Equal(DXColumnTypeEnum.GUID, objectIdColumn.ColumnType);

            var systemTimeStampColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.Name == "TimeStamp");
            Assert.NotNull(systemTimeStampColumn);
            Assert.Equal(DateTime.UtcNow, systemTimeStampColumn.TimeStamp, difference);
            Assert.Equal("TimeStamp", systemTimeStampColumn.Name);
            Assert.Equal(DXColumnTypeEnum.TimeStamp, systemTimeStampColumn.ColumnType);

            var guidColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("5BCDF497-6004-4028-BB18-5185576E2094"));
            Assert.NotNull(guidColumn);
            Assert.Equal(new Guid("5BCDF497-6004-4028-BB18-5185576E2094"), guidColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), guidColumn.ObjectID);
            Assert.Equal("GuidColumn", guidColumn.Name);
            Assert.Equal(DXColumnTypeEnum.GUID, guidColumn.ColumnType);

            var timeStampColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("C44DB212-4612-4367-8FBF-B5826667EA4C"));
            Assert.NotNull(timeStampColumn);
            Assert.Equal(new Guid("C44DB212-4612-4367-8FBF-B5826667EA4C"), timeStampColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), timeStampColumn.ObjectID);
            Assert.Equal("TimeStampColumn", timeStampColumn.Name);
            Assert.Equal(DXColumnTypeEnum.TimeStamp, timeStampColumn.ColumnType);
            Assert.Equal("CURRENT_TIMESTAMP", timeStampColumn.DefaultValue);
        }

        [Fact]
        public void UpdateDataDXElement_RemoveAndAddSeveralColumns_Success()
        {
            // Init
            string json0 = File.ReadAllText("Assets/JSON/DXElementDefinitionUnit/DXElementDefinitionUnit0000.json");
            string json2 = File.ReadAllText("Assets/JSON/DXElementDefinitionUnit/DXElementDefinitionUnit0002.json");
            string json3 = File.ReadAllText("Assets/JSON/DXElementDefinitionUnit/DXElementDefinitionUnit0003.json");

            DXElementDefinitionUnit dxElementDesc0 = DXUnitHelper.CreateInstance<DXElementDefinitionUnit>(json0);
            DXElementDefinitionUnit dxElementDesc2 = DXUnitHelper.CreateInstance<DXElementDefinitionUnit>(json2);
            DXElementDefinitionUnit dxElementDesc3 = DXUnitHelper.CreateInstance<DXElementDefinitionUnit>(json3);

            base._finalizationAction = new Action(() =>
            {
                this._dataService.DeleteAsync(dxElementDesc3).Wait();
            });

            // Action
            this._dataService.InsertAsync(dxElementDesc0).Wait();
            this._dataService.UpdateAsync(dxElementDesc2).Wait();
            this._dataService.UpdateAsync(dxElementDesc3).Wait();

            // Checking
            var dxElementDefinition = this._genericRepo.GetDXUnit<DXElementDefinitionUnit>(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"));

            Assert.NotNull(dxElementDefinition);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), dxElementDefinition.ID);

            Assert.NotNull(dxElementDefinition.DXObjectDefinitionMainElement);
            Assert.Equal(new Guid("EB217F3B-1CC3-4CB0-8B5C-E7C71AEDACB4"), dxElementDefinition.DXObjectDefinitionMainElement.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), dxElementDefinition.DXObjectDefinitionMainElement.ObjectID);
            Assert.Equal("NewDataDXElement", dxElementDefinition.DXObjectDefinitionMainElement.Name);
            Assert.True(dxElementDefinition is DXElementDefinitionUnit);

            Assert.NotNull(dxElementDefinition.DXColumnDefinitionElement);
            Assert.Equal(7, dxElementDefinition.DXColumnDefinitionElement.Announced.Count());

            var idColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.Name == "ID");
            Assert.NotNull(idColumn);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), idColumn.ObjectID);
            Assert.Equal("ID", idColumn.Name);
            Assert.Equal(DXColumnTypeEnum.GUID, idColumn.ColumnType);

            var objectIdColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.Name == "ObjectID");
            Assert.NotNull(objectIdColumn);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), objectIdColumn.ObjectID);
            Assert.Equal("ObjectID", objectIdColumn.Name);
            Assert.Equal(DXColumnTypeEnum.GUID, objectIdColumn.ColumnType);

            var systemTimeStampColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.Name == "TimeStamp");
            Assert.NotNull(systemTimeStampColumn);
            Assert.Equal(DateTime.UtcNow, systemTimeStampColumn.TimeStamp, difference);
            Assert.Equal("TimeStamp", systemTimeStampColumn.Name);
            Assert.Equal(DXColumnTypeEnum.TimeStamp, systemTimeStampColumn.ColumnType);

            var guidColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("5BCDF497-6004-4028-BB18-5185576E2094"));
            Assert.NotNull(guidColumn);
            Assert.Equal(new Guid("5BCDF497-6004-4028-BB18-5185576E2094"), guidColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), guidColumn.ObjectID);
            Assert.Equal("GuidColumn", guidColumn.Name);
            Assert.Equal(DXColumnTypeEnum.GUID, guidColumn.ColumnType);

            var timeStampColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("C44DB212-4612-4367-8FBF-B5826667EA4C"));
            Assert.NotNull(timeStampColumn);
            Assert.Equal(new Guid("C44DB212-4612-4367-8FBF-B5826667EA4C"), timeStampColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), timeStampColumn.ObjectID);
            Assert.Equal("TimeStampColumn", timeStampColumn.Name);
            Assert.Equal(DXColumnTypeEnum.TimeStamp, timeStampColumn.ColumnType);
            Assert.Equal("CURRENT_TIMESTAMP", timeStampColumn.DefaultValue);

            var currencyColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("3736BFE6-BF1E-41C3-A72C-A0CA073B1F38"));
            Assert.NotNull(currencyColumn);
            Assert.Equal(new Guid("3736BFE6-BF1E-41C3-A72C-A0CA073B1F38"), currencyColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), currencyColumn.ObjectID);
            Assert.Equal("CurrencyColumn", currencyColumn.Name);
            Assert.Equal(DXColumnTypeEnum.Currency, currencyColumn.ColumnType);
            Assert.True(currencyColumn.AllowNull);
            Assert.Equal("0", currencyColumn.DefaultValue);

            var blobColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("029B39F4-CF55-49D5-876B-6C69C633B856"));
            Assert.NotNull(blobColumn);
            Assert.Equal(new Guid("029B39F4-CF55-49D5-876B-6C69C633B856"), blobColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), blobColumn.ObjectID);
            Assert.Equal("BlobColumn", blobColumn.Name);
            Assert.Equal(DXColumnTypeEnum.Blob, blobColumn.ColumnType);
            Assert.True(blobColumn.AllowNull);
        }

        [Fact]
        public void UpdateDataDXElement_RemoveAndAddAndUpdateSeveralColumns_Success()
        {
            // Init
            string json0 = File.ReadAllText("Assets/JSON/DXElementDefinitionUnit/DXElementDefinitionUnit0000.json");
            string json4 = File.ReadAllText("Assets/JSON/DXElementDefinitionUnit/DXElementDefinitionUnit0004.json");
            DXElementDefinitionUnit dxElementDesc0 = DXUnitHelper.CreateInstance<DXElementDefinitionUnit>(json0);
            DXElementDefinitionUnit dxElementDesc4 = DXUnitHelper.CreateInstance<DXElementDefinitionUnit>(json4);

            base._finalizationAction = new Action(() =>
            {
                this._dataService.DeleteAsync(dxElementDesc4).Wait();
            });

            // Action
            this._dataService.InsertAsync(dxElementDesc0).Wait();
            this._dataService.UpdateAsync(dxElementDesc4).Wait();

            // Checking
            var dxElementDefinition = this._genericRepo.GetDXUnit<DXElementDefinitionUnit>(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"));

            Assert.NotNull(dxElementDefinition);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), dxElementDefinition.ID);

            Assert.NotNull(dxElementDefinition.DXObjectDefinitionMainElement);
            Assert.Equal(new Guid("EB217F3B-1CC3-4CB0-8B5C-E7C71AEDACB4"), dxElementDefinition.DXObjectDefinitionMainElement.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), dxElementDefinition.DXObjectDefinitionMainElement.ObjectID);
            Assert.Equal("UpdatedDataDXElement", dxElementDefinition.DXObjectDefinitionMainElement.Name);
            Assert.True(dxElementDefinition is DXElementDefinitionUnit);

            Assert.NotNull(dxElementDefinition.DXColumnDefinitionElement);
            Assert.Equal(8, dxElementDefinition.DXColumnDefinitionElement.Announced.Count());

            var idColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.Name == "ID");
            Assert.NotNull(idColumn);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), idColumn.ObjectID);
            Assert.Equal("ID", idColumn.Name);
            Assert.Equal(DXColumnTypeEnum.GUID, idColumn.ColumnType);

            var objectIdColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.Name == "ObjectID");
            Assert.NotNull(objectIdColumn);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), objectIdColumn.ObjectID);
            Assert.Equal("ObjectID", objectIdColumn.Name);
            Assert.Equal(DXColumnTypeEnum.GUID, objectIdColumn.ColumnType);

            var systemTimeStampColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.Name == "TimeStamp");
            Assert.NotNull(systemTimeStampColumn);
            Assert.Equal(DateTime.UtcNow, systemTimeStampColumn.TimeStamp, difference);
            Assert.Equal("TimeStamp", systemTimeStampColumn.Name);
            Assert.Equal(DXColumnTypeEnum.TimeStamp, systemTimeStampColumn.ColumnType);

            var guidColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("5BCDF497-6004-4028-BB18-5185576E2094"));
            Assert.NotNull(guidColumn);
            Assert.Equal(new Guid("5BCDF497-6004-4028-BB18-5185576E2094"), guidColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), guidColumn.ObjectID);
            Assert.Equal("GuidColumn", guidColumn.Name);
            Assert.Equal(DXColumnTypeEnum.GUID, guidColumn.ColumnType);

            var timeStampColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("C44DB212-4612-4367-8FBF-B5826667EA4C"));
            Assert.NotNull(timeStampColumn);
            Assert.Equal(new Guid("C44DB212-4612-4367-8FBF-B5826667EA4C"), timeStampColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), timeStampColumn.ObjectID);
            Assert.Equal("TimeStampColumn", timeStampColumn.Name);
            Assert.Equal(DXColumnTypeEnum.TimeStamp, timeStampColumn.ColumnType);
            Assert.Equal("CURRENT_TIMESTAMP", timeStampColumn.DefaultValue);

            var stringColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("E042EF31-397E-4614-BABC-79132D4A68DF"));
            Assert.NotNull(stringColumn);
            Assert.Equal(new Guid("E042EF31-397E-4614-BABC-79132D4A68DF"), stringColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), stringColumn.ObjectID);
            Assert.Equal("StringColumnNew", stringColumn.Name);
            Assert.Equal(DXColumnTypeEnum.String, stringColumn.ColumnType);
            Assert.False(stringColumn.AllowNull);
            Assert.Equal(200, stringColumn.Length);
            Assert.Equal("'StringValueNew'", stringColumn.DefaultValue);

            var currencyColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("3736BFE6-BF1E-41C3-A72C-A0CA073B1F38"));
            Assert.NotNull(currencyColumn);
            Assert.Equal(new Guid("3736BFE6-BF1E-41C3-A72C-A0CA073B1F38"), currencyColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), currencyColumn.ObjectID);
            Assert.Equal("CurrencyColumnUpdated", currencyColumn.Name);
            Assert.Equal(DXColumnTypeEnum.Currency, currencyColumn.ColumnType);
            Assert.False(currencyColumn.AllowNull);
            Assert.Equal("1", currencyColumn.DefaultValue);

            var blobColumn = dxElementDefinition.DXColumnDefinitionElement.Announced.SingleOrDefault(x => x.ID == new Guid("029B39F4-CF55-49D5-876B-6C69C633B856"));
            Assert.NotNull(blobColumn);
            Assert.Equal(new Guid("029B39F4-CF55-49D5-876B-6C69C633B856"), blobColumn.ID);
            Assert.Equal(new Guid("7989B845-6AAA-4ADB-99ED-B4F0840348F8"), blobColumn.ObjectID);
            Assert.Equal("BlobColumnUpdated", blobColumn.Name);
            Assert.Equal(DXColumnTypeEnum.Blob, blobColumn.ColumnType);
            Assert.False(blobColumn.AllowNull);
        }
    }
}