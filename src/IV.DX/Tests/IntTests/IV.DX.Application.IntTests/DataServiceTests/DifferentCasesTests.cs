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
        IDXUnitDataService _dataService;
        IDXUnitGenericRepository _genericRepo;

        public DifferentCasesTests(DXTestFixture fx, ITestOutputHelper output)
            : base(fx, output)
        {
            this._dataService = this.ServiceProvider.GetRequiredService<IDXUnitDataService>();
            this._genericRepo = this.ServiceProvider.GetRequiredService<IDXUnitGenericRepository>();
        }

        [Fact]
        public void UpdateDXUnitDefinition_UsingTargetModeForColumnDefinitionWithEmptyDefinitions_Ok()
        {
            // Init
            var dxElementDescObjectID = new Guid("f0ff00d1-303e-42e6-9769-e482b0bf79ff");
            var dxUnitDescObjectID = new Guid("f0ff00d1-303e-42e6-9769-e482b0bf79aa");
            var testDXUnitID = new Guid("aaff00d1-303e-42e6-9769-e482b0bf79ff");

            var intCln = new DXColumnDefinitionElement()
            {
                ID = Guid.NewGuid(),
                ObjectID = dxElementDescObjectID,
                Name = "IntCln",
                ColumnType = DXColumnTypeEnum.Int
            };

            var strCln = new DXColumnDefinitionElement()
            {
                ID = Guid.NewGuid(),
                Name = "StrCln",
                Length = 100,
                DefaultValue = "''",
                ColumnType = DXColumnTypeEnum.String
            };

            DXElementDefinitionUnit dxElementDescObject = new DXElementDefinitionUnit()
            {
                ID = dxElementDescObjectID,
                DXObjectDefinitionMainElement = new DXObjectDefinitionMainElement()
                {
                    ID = Guid.NewGuid(),
                    ObjectID = dxElementDescObjectID,
                    Name = "TestDXElement"
                },
                DXColumnDefinitionElement = new DXMultiElementsContainer<DXColumnDefinitionElement>()
                {
                    Mode = MultiElementsMode.Target,
                    Announced = new List<DXColumnDefinitionElement>()
                    {
                        intCln
                    }
                }
            };

            DXUnitDefinitionUnit dxUnitDescObject = new DXUnitDefinitionUnit()
            {
                ID = dxUnitDescObjectID,
                DXObjectDefinitionMainElement = new DXObjectDefinitionMainElement()
                {
                    ID = Guid.NewGuid(),
                    ObjectID = dxUnitDescObjectID,
                    Name = "TestDXUnit"
                },
                DXElementInUnitDefinitionElement = new DXMultiElementsContainer<DXElementInUnitDefinitionElement>()
                {
                    Announced = new List<DXElementInUnitDefinitionElement>()
                    {
                        new DXElementInUnitDefinitionElement()
                        {
                            ID = Guid.NewGuid(),
                            ObjectID = dxUnitDescObjectID,
                            RelationType = DXElementInUnitTypeEnum.SingleMandatory,
                            DXElementDefinitionUnit = dxElementDescObject.ID
                        }
                    }
                }
            };

            var item = new TestDXUnit()
            {
                ID = testDXUnitID,
                TestDXElement = new TestDXElement()
                {
                    ID = Guid.NewGuid(),
                    ObjectID = testDXUnitID,
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
                Announced = new List<DXColumnDefinitionElement>()
                {
                    strCln
                },
                Deleted = new List<DXColumnDefinitionElement>()
                {
                    intCln
                }
            };

            this._dataService.InsertOrUpdateAsync(dxElementDescObject).Wait();

            // Assert
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
            var jObject = await this._dataService.GetItemAsync("TDeviceUnit", id);

            // Assert
            Assert.Null(jObject["User"]);
        }

        [Fact]
        public async Task UpdateDXUnit_UsingAddedDeletedDXElementsWithTargetMode_Ok()
        {
            // Init
            var dxUnitID = new Guid("622c2056-9797-47ab-82c2-5c3eeb6a68ce");
            var dxElementToAddID = new Guid("d3b5e1e2-3f3a-4f7c-8f0c-5e2b8e6f4a1c");
            var dxElementToDeleteID = new Guid("4b95f498-f0cb-407d-be16-e7a1518fc070");

            var dxElementToAdd = new DXElementDefinitionUnit()
            {
                ID = dxElementToAddID,
                DXObjectDefinitionMainElement = new DXObjectDefinitionMainElement()
                {
                    ID = Guid.NewGuid(),
                    ObjectID = dxElementToAddID,
                    Name = "TestDXElementToAdde7a1518fc070"
                }
            };

            var dxElementToDelete = new DXElementDefinitionUnit()
            {
                ID = dxElementToDeleteID,
                DXObjectDefinitionMainElement = new DXObjectDefinitionMainElement()
                {
                    ID = Guid.NewGuid(),
                    ObjectID = dxElementToDeleteID,
                    Name = "TestDXElementToDelete5e2b8e6f4a1c"
                }
            };

            var dxUnit = new DXUnitDefinitionUnit()
            {
                ID = dxUnitID,
                DXObjectDefinitionMainElement = new DXObjectDefinitionMainElement()
                {
                    ID = Guid.NewGuid(),
                    ObjectID = dxUnitID,
                    Name = "TestDXUnit5c3eeb6a68ce"
                }
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
                Announced = new List<DXElementInUnitDefinitionElement>()
                {
                    new DXElementInUnitDefinitionElement()
                    {
                        ID = Guid.NewGuid(),
                        RelationType = DXElementInUnitTypeEnum.SingleMandatory,
                        DXElementDefinitionUnit = dxElementToAdd.ID
                    }
                },
                Deleted = new List<DXElementInUnitDefinitionElement>()
                {
                    new DXElementInUnitDefinitionElement()
                    {
                        ID = Guid.NewGuid(),
                        RelationType = DXElementInUnitTypeEnum.SingleMandatory,
                        DXElementDefinitionUnit = dxElementToDelete.ID
                    }
                }
            };

            this._dataService.UpdateAsync(dxUnit).Wait();

            // Assert
            var existingDXUnit = await this._dataService.GetItemAsync<DXUnitDefinitionUnit>(dxUnitID);

            Assert.Single(existingDXUnit.DXElementInUnitDefinitionElement.Announced);

            var announcedDXElement = existingDXUnit.DXElementInUnitDefinitionElement.Announced.Single();

            Assert.Equal(dxElementToAdd.ID, announcedDXElement.DXElementDefinitionUnit);

            // Action
            existingDXUnit.DXElementInUnitDefinitionElement = new DXMultiElementsContainer<DXElementInUnitDefinitionElement>()
            {
                Mode = MultiElementsMode.Target,
                Deleted = new List<DXElementInUnitDefinitionElement>()
                {
                    announcedDXElement
                }
            };

            this._dataService.UpdateAsync(existingDXUnit).Wait();

            // Assert
            existingDXUnit = await this._dataService.GetItemAsync<DXUnitDefinitionUnit>(dxUnitID);
            Assert.Empty(existingDXUnit.DXElementInUnitDefinitionElement.Announced);
        }


        [Fact]
        public async Task DeleteDXUnit_WithDXElements_Ok()
        {
            // Init
            var dxUnitID = new Guid("1c4e8f3e-3f4b-4c6a-9f7e-8f9e7d6c5b4a");
            var dxElementID = new Guid("02449441-f8c4-483a-950f-6b47f2f216b4");

            var dxElement = new DXElementDefinitionUnit()
            {
                ID = dxElementID,
                DXObjectDefinitionMainElement = new DXObjectDefinitionMainElement()
                {
                    ID = Guid.NewGuid(),
                    ObjectID = dxElementID,
                    Name = "TestDXElement6b47f2f216b4"
                }
            };

            var dxUnit = new DXUnitDefinitionUnit()
            {
                ID = dxUnitID,
                DXObjectDefinitionMainElement = new DXObjectDefinitionMainElement()
                {
                    ID = Guid.NewGuid(),
                    ObjectID = dxUnitID,
                    Name = "TestDXUnit8f9e7d6c5b4a"
                },
                DXElementInUnitDefinitionElement = new DXMultiElementsContainer<DXElementInUnitDefinitionElement>()
                {
                    Announced = new List<DXElementInUnitDefinitionElement>()
                    {
                        new DXElementInUnitDefinitionElement()
                        {
                            ID = Guid.NewGuid(),
                            RelationType = DXElementInUnitTypeEnum.SingleMandatory,
                            DXElementDefinitionUnit = dxElement.ID
                        }
                    }
                }
            };

            base._finalizationAction = () =>
            {
                this._dataService.DeleteAsync(dxUnit).Wait();
                this._dataService.DeleteAsync(dxElement).Wait();
            };

            this._dataService.InsertAsync(dxElement).Wait();
            this._dataService.InsertAsync(dxUnit).Wait();

            // Action
            this._dataService.DeleteAsync(dxUnit).Wait();

            // Assert
            var existingDXUnit = await this._dataService.GetItemAsync<DXUnitDefinitionUnit>(dxUnitID);
            Assert.Null(existingDXUnit);

            var existingDXElement = await this._dataService.GetItemAsync<DXElementDefinitionUnit>(dxElement.ID);
            Assert.NotNull(existingDXElement);
        }

        [Fact]
        public async Task UpdateDXUnit_UsingMoreThanOnelDeletedDXElementsWithTargetMode_Ok()
        {
            // Init
            var dxUnitID = new Guid("1873aa67-8f3e-4044-8659-c44f7a2dd5f6");
            var dxElementID1 = new Guid("f8f68404-6143-433e-aa2c-f45215f6be1c");
            var dxElementID2 = new Guid("63c28d39-1561-4c97-b212-fa7db5443a11");
            var dxElementID3 = new Guid("8ae093df-d6b8-4d13-acc7-801b464bfb0f");


            var dxElement1 = new DXElementDefinitionUnit()
            {
                ID = dxElementID1,
                DXObjectDefinitionMainElement = new DXObjectDefinitionMainElement()
                {
                    ID = Guid.NewGuid(),
                    ObjectID = dxElementID1,
                    Name = "TestDXElementf45215f6be1c"
                }
            };

            var dxElement2 = new DXElementDefinitionUnit()
            {
                ID = dxElementID2,
                DXObjectDefinitionMainElement = new DXObjectDefinitionMainElement()
                {
                    ID = Guid.NewGuid(),
                    ObjectID = dxElementID2,
                    Name = "TestDXElementfa7db5443a11"
                }
            };

            var dxElement3 = new DXElementDefinitionUnit()
            {
                ID = dxElementID3,
                DXObjectDefinitionMainElement = new DXObjectDefinitionMainElement()
                {
                    ID = Guid.NewGuid(),
                    ObjectID = dxElementID3,
                    Name = "TestDXElement801b464bfb0f"
                }
            };

            var dxUnit = new DXUnitDefinitionUnit()
            {
                ID = dxUnitID,
                DXObjectDefinitionMainElement = new DXObjectDefinitionMainElement()
                {
                    ID = Guid.NewGuid(),
                    ObjectID = dxUnitID,
                    Name = "TestDXUnitc44f7a2dd5f6"
                },
                DXElementInUnitDefinitionElement = new DXMultiElementsContainer<DXElementInUnitDefinitionElement>()
                {
                    Announced = new List<DXElementInUnitDefinitionElement>()
                    {
                        new DXElementInUnitDefinitionElement()
                        {
                            ID = Guid.NewGuid(),
                            ObjectID = dxUnitID,
                            RelationType = DXElementInUnitTypeEnum.SingleMandatory,
                            DXElementDefinitionUnit = dxElement1.ID
                        },
                        new DXElementInUnitDefinitionElement()
                        {
                            ID = Guid.NewGuid(),
                            ObjectID = dxUnitID,
                            RelationType = DXElementInUnitTypeEnum.SingleMandatory,
                            DXElementDefinitionUnit = dxElement2.ID
                        },
                        new DXElementInUnitDefinitionElement()
                        {
                            ID = Guid.NewGuid(),
                            ObjectID = dxUnitID,
                            RelationType = DXElementInUnitTypeEnum.SingleMandatory,
                            DXElementDefinitionUnit = dxElement3.ID
                        }
                    }
                }
            };

            base._finalizationAction = () =>
            {
                this._dataService.DeleteAsync(dxUnit).Wait();
                this._dataService.DeleteAsync(dxElement1).Wait();
                this._dataService.DeleteAsync(dxElement2).Wait();
                this._dataService.DeleteAsync(dxElement3).Wait();
            };

            this._dataService.InsertAsync(dxElement1).Wait();
            this._dataService.InsertAsync(dxElement2).Wait();
            this._dataService.InsertAsync(dxElement3).Wait();
            this._dataService.InsertAsync(dxUnit).Wait();

            // Action
            dxUnit.DXElementInUnitDefinitionElement = new DXMultiElementsContainer<DXElementInUnitDefinitionElement>()
            {
                Mode = MultiElementsMode.Target,
                Deleted = dxUnit.DXElementInUnitDefinitionElement.Announced,
                Announced = new List<DXElementInUnitDefinitionElement>()
            };

            this._dataService.UpdateAsync(dxUnit).Wait();

            // Assert
            var existingDXUnit = await this._dataService.GetItemAsync<DXUnitDefinitionUnit>(dxUnitID);
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