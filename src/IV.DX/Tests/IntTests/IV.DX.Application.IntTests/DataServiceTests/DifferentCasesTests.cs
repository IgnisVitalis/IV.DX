using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using IV.DX.Shared.IntTests;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace IV.DX.Application.IntTests.DataServiceTests
{
    [Collection("DX:one-time")]
    public class DifferentCasesTests : IntTestController
    {
        private readonly IDXUnitDataService _dataService;
        private readonly IDXUnitDataReader _dataReader;
        private readonly IDXUnitGenericRepository _genericRepo;

        public DifferentCasesTests(DXTestFixture fx, ITestOutputHelper output)
            : base(fx, output)
        {
            _dataService = this.ServiceProvider.GetRequiredService<IDXUnitDataService>();
            _dataReader = this.ServiceProvider.GetRequiredService<IDXUnitDataReader>();
            _genericRepo = this.ServiceProvider.GetRequiredService<IDXUnitGenericRepository>();
        }

        [Fact]
        public void UpdateDXUnitDefinition_UsingTargetModeForColumnDefinitionWithEmptyDefinitions_Ok()
        {
            // Init
            var intCln = new DXColumnDefinitionElement()
            {
                Name = "IntCln",
                ColumnType = DXColumnTypeEnum.Int
            };

            var strCln = new DXColumnDefinitionElement()
            {
                Id = Guid.NewGuid(),
                Name = "StrCln",
                Length = 100,
                DefaultValue = "''",
                ColumnType = DXColumnTypeEnum.String
            };

            DXElementDefinitionUnit dxElementDescObject = new DXElementDefinitionUnit()
            {
                Name = "TestDXElement",
                DXTitleExpression = "Id",

                DXColumnDefinitionElement = new DXMultiElementsContainer<DXColumnDefinitionElement>()
                {
                    Mode = MultiElementsMode.Target,
                    Announced = new HashSet<DXColumnDefinitionElement>()
                    {
                        intCln
                    }
                }
            };

            DXUnitDefinitionUnit dxUnitDescObject = new DXUnitDefinitionUnit()
            {
                Name = "TestDXUnit",
                DXTitleExpression = "Id"
            };

            var item = new TestDXUnit()
            {
                TestDXElement = new TestDXElement()
                {
                    IntCln = 123
                }
            };

            base._finalizationAction = () =>
            {
                base.RunActionSafety(() =>
                {
                    this._dataService.DeleteAsync(item).Wait();
                });

                this._dataService.UpdateAsync(dxUnitDescObject).Wait();
                this._dataService.DeleteAsync(dxUnitDescObject).Wait();
                this._dataService.DeleteAsync(dxElementDescObject).Wait();
            };

            // Action
            this._dataService.InsertOrUpdateAsync(dxElementDescObject).Wait();

            dxUnitDescObject.DXElementInUnitDefinitionElement = new DXMultiElementsContainer<DXElementInUnitDefinitionElement>()
            {
                Announced = new HashSet<DXElementInUnitDefinitionElement>()
                {
                    new DXElementInUnitDefinitionElement()
                    {
                        RelationType = DXElementInUnitTypeEnum.SingleMandatory,
                        DXElementDefinitionUnit = dxElementDescObject.Id
                    }
                }
            };

            this._dataService.InsertOrUpdateAsync(dxUnitDescObject).Wait();

            this._dataService.InsertOrUpdateAsync(item).Wait();

            // Assert
            var existingItems = this._genericRepo.GetDXUnits<TestDXUnit>();

            Assert.Single(existingItems);

            var existingItem = existingItems.Single();

            Assert.Equal(item.TestDXElement.IntCln, existingItem.TestDXElement.IntCln);

            // Action
            dxElementDescObject.DXColumnDefinitionElement = new DXMultiElementsContainer<DXColumnDefinitionElement>()
            {
                Mode = MultiElementsMode.Target,
                Announced = new HashSet<DXColumnDefinitionElement>()
                {
                    strCln
                },
                Deleted = new HashSet<DXColumnDefinitionElement>()
                {
                    intCln
                }
            };

            this._dataService.InsertOrUpdateAsync(dxElementDescObject).Wait();

            // Assert
            var cache = this.ServiceProvider.GetRequiredService<IDXStructureCache>();
            cache.RefreshAsync().Wait();

            var existingModifiedItems = this._genericRepo.GetDXUnits<TestDXUnitModified>();

            Assert.Single(existingItems);

            var existingItemModified = existingModifiedItems.Single();

            Assert.Equal("", existingItemModified.TestDXElement.StrCln);
        }

        [Fact]
        public async Task GetItem_UsingMultidxElementWithRelation_Ok()
        {
            // Init
            var id = new Guid("a03f744d-d5db-4d4e-95a8-d5fbf4bad2d7");

            // Action
            var jObject = await _dataReader.GetItemAsync("TDeviceUnit", id);

            // Assert
            var block = jObject.ToObject<DXDataBlock<DXUnitRecord>>();
            var record = block?.Data?.Items?.SingleOrDefault();
            var user = record?.Fields?["User"]?.ToObject<string>();

            Assert.NotNull(user);
            Assert.Equal("018fa54a-203e-7407-9bd0-cd287e850b03", user);
        }

        [Fact]
        public async Task UpdateDXUnit_UsingAddedDeletedDXElementsWithTargetMode_Ok()
        {
            // Init
            var dxElementToAdd = new DXElementDefinitionUnit()
            {
                Name = "TestDXElementToAdde7a1518fc070",
                DXTitleExpression = "Name"
            };

            var dxElementToDelete = new DXElementDefinitionUnit()
            {
                Name = "TestDXElementToDelete5e2b8e6f4a1c",
                DXTitleExpression = "Name"
            };

            var dxUnit = new DXUnitDefinitionUnit()
            {
                Name = "TestDXUnit5c3eeb6a68ce",
                DXTitleExpression = "Name"
            };

            base._finalizationAction = () =>
            {
                this._dataService.DeleteAsync(dxUnit).Wait();
                this._dataService.DeleteAsync(dxElementToAdd).Wait();
                this._dataService.DeleteAsync(dxElementToDelete).Wait();
            };

            this._dataService.InsertAsync(dxUnit).Wait();
            this._dataService.InsertAsync(dxElementToAdd).Wait();
            this._dataService.InsertAsync(dxElementToDelete).Wait();

            // Action
            dxUnit.DXElementInUnitDefinitionElement = new DXMultiElementsContainer<DXElementInUnitDefinitionElement>()
            {
                Mode = MultiElementsMode.Target,
                Announced = new HashSet<DXElementInUnitDefinitionElement>()
                {
                    new DXElementInUnitDefinitionElement()
    {
        Id = Guid.NewGuid(),
                        RelationType = DXElementInUnitTypeEnum.SingleMandatory,
                        DXElementDefinitionUnit = dxElementToAdd.Id
                    }
},
                Deleted = new HashSet<DXElementInUnitDefinitionElement>()
                {
                    new DXElementInUnitDefinitionElement()
                    {
                        Id = Guid.NewGuid(),
                        RelationType = DXElementInUnitTypeEnum.SingleMandatory,
                        DXElementDefinitionUnit = dxElementToDelete.Id
                    }
                }
            };

            this._dataService.UpdateAsync(dxUnit).Wait();

            // Assert
            var existingDXUnit = await _dataReader.GetItemAsync<DXUnitDefinitionUnit>(dxUnit.Id);

            Assert.Single(existingDXUnit.DXElementInUnitDefinitionElement.Announced);

            var announcedDXElement = existingDXUnit.DXElementInUnitDefinitionElement.Announced.Single();

            Assert.Equal(dxElementToAdd.Id, announcedDXElement.DXElementDefinitionUnit);

            // Action
            existingDXUnit.DXElementInUnitDefinitionElement = new DXMultiElementsContainer<DXElementInUnitDefinitionElement>()
            {
                Mode = MultiElementsMode.Target,
                Deleted = new HashSet<DXElementInUnitDefinitionElement>()
                {
                    announcedDXElement
                }
            };

            this._dataService.UpdateAsync(existingDXUnit).Wait();

            // Assert
            existingDXUnit = await _dataReader.GetItemAsync<DXUnitDefinitionUnit>(dxUnit.Id);
            Assert.Empty(existingDXUnit.DXElementInUnitDefinitionElement.Announced);
        }


        [Fact]
        public async Task DeleteDXUnit_WithDXElements_Ok()
        {
            // Init
            var dxElement = new DXElementDefinitionUnit()
            {
                Name = "TestDXElement6b47f2f216b4",
                DXTitleExpression = "Name"
            };

            DXUnitDefinitionUnit dxUnit = null;

            base._finalizationAction = () =>
            {
                if (dxUnit != null) this._dataService.DeleteAsync(dxUnit).Wait();
                this._dataService.DeleteAsync(dxElement).Wait();
            };

            this._dataService.InsertAsync(dxElement).Wait();

            dxUnit = new DXUnitDefinitionUnit()
            {
                Name = "TestDXUnit8f9e7d6c5b4a",
                DXTitleExpression = "Name",

                DXElementInUnitDefinitionElement = new DXMultiElementsContainer<DXElementInUnitDefinitionElement>()
                {
                    Announced = new HashSet<DXElementInUnitDefinitionElement>()
                    {
                        new DXElementInUnitDefinitionElement()
                        {
                            RelationType = DXElementInUnitTypeEnum.SingleMandatory,
                            DXElementDefinitionUnit = dxElement.Id
                        }
                    }
                }
            };

            this._dataService.InsertAsync(dxUnit).Wait();

            // Action
            this._dataService.DeleteAsync(dxUnit).Wait();

            // Assert
            var existingDXUnit = await _dataReader.GetItemAsync<DXUnitDefinitionUnit>(dxUnit.Id);
            Assert.Null(existingDXUnit);

            var existingDXElement = await _dataReader.GetItemAsync<DXElementDefinitionUnit>(dxElement.Id);
            Assert.NotNull(existingDXElement);
        }

        [Fact]
        public async Task UpdateDXUnit_UsingMoreThanOnelDeletedDXElementsWithTargetMode_Ok()
        {
            // Init
            var dxElement1 = new DXElementDefinitionUnit()
            {
                DXTitleExpression = "Name",
                Name = "TestDXElementf45215f6be1c"
            };

            var dxElement2 = new DXElementDefinitionUnit()
            {
                Name = "TestDXElementfa7db5443a11",
                DXTitleExpression = "Name"
            };

            var dxElement3 = new DXElementDefinitionUnit()
            {
                Name = "TestDXElement801b464bfb0f",
                DXTitleExpression = "Name"
            };

            DXUnitDefinitionUnit dxUnit = null;

            base._finalizationAction = () =>
            {
                if (dxUnit != null) this._dataService.DeleteAsync(dxUnit).Wait();
                this._dataService.DeleteAsync(dxElement1).Wait();
                this._dataService.DeleteAsync(dxElement2).Wait();
                this._dataService.DeleteAsync(dxElement3).Wait();
            };

            this._dataService.InsertAsync(dxElement1).Wait();
            this._dataService.InsertAsync(dxElement2).Wait();
            this._dataService.InsertAsync(dxElement3).Wait();

            dxUnit = new DXUnitDefinitionUnit()
            {
                Name = "TestDXUnitc44f7a2dd5f6",
                DXTitleExpression = "Name",

                DXElementInUnitDefinitionElement = new DXMultiElementsContainer<DXElementInUnitDefinitionElement>()
                {
                    Announced = new HashSet<DXElementInUnitDefinitionElement>()
                    {
                        new DXElementInUnitDefinitionElement()
                        {
                            RelationType = DXElementInUnitTypeEnum.SingleMandatory,
                            DXElementDefinitionUnit = dxElement1.Id
                        },
                        new DXElementInUnitDefinitionElement()
                        {
                            RelationType = DXElementInUnitTypeEnum.SingleMandatory,
                            DXElementDefinitionUnit = dxElement2.Id
                        },
                        new DXElementInUnitDefinitionElement()
                        {
                            RelationType = DXElementInUnitTypeEnum.SingleMandatory,
                            DXElementDefinitionUnit = dxElement3.Id
                        }
                    }
                }
            };

            this._dataService.InsertAsync(dxUnit).Wait();

            // Action
            dxUnit.DXElementInUnitDefinitionElement = new DXMultiElementsContainer<DXElementInUnitDefinitionElement>()
            {
                Mode = MultiElementsMode.Target,
                Deleted = dxUnit.DXElementInUnitDefinitionElement.Announced,
                Announced = new HashSet<DXElementInUnitDefinitionElement>()
            };

            this._dataService.UpdateAsync(dxUnit).Wait();

            // Assert
            var existingDXUnit = await _dataReader.GetItemAsync<DXUnitDefinitionUnit>(dxUnit.Id);
            Assert.Empty(existingDXUnit.DXElementInUnitDefinitionElement.Announced);
        }
    }

    [DXUnit("TestDXUnit")]
    public class TestDXUnit : DXUnit
    {
        public TestDXElement TestDXElement { get; set; }
    }

    [DXElement("TestDXElement")]
    public class TestDXElement : DXElement
    {
        [DXColumn("IntCln")]
        public int IntCln { get; set; }
    }

    [DXUnit("TestDXUnit")]
    public class TestDXUnitModified : DXUnit
    {
        public TestDXElementModified TestDXElement { get; set; }
    }

    [DXElement("TestDXElement")]
    public class TestDXElementModified : DXElement
    {
        [DXColumn("StrCln")]
        public string StrCln { get; set; }
    }
}

