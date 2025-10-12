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
        public void UpdateEntityDefinition_UsingTargetModeForColumnDefinitionWithEmptyDefinitions_Ok()
        {
            // Init
            var id = new Guid("f0ff00d1-303e-42e6-9769-e482b0bf79ff");

            var intCln = new DXColumnDefinitionElement()
            {
                ID = Guid.NewGuid(),
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

            DXElementDefinitionUnit blockDescObject = new DXElementDefinitionUnit()
            {
                ID = id,
                DXUnitDefinitionMainElement = new DXUnitDefinitionMainElement()
                {
                    ID = Guid.NewGuid(),
                    Name = "TestBlock"
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
                ID = Guid.NewGuid(),
                DXUnitDefinitionMainElement = new DXUnitDefinitionMainElement()
                {
                    ID = Guid.NewGuid(),
                    Name = "TestEntity"
                },
                DXElementInUnitDefinitionMainElement = new DXMultiElementsContainer<DXElementInUnitDefinitionMainElement>()
                {
                    Announced = new List<DXElementInUnitDefinitionMainElement>()
                    {
                        new DXElementInUnitDefinitionMainElement()
                        {
                            ID = Guid.NewGuid(),
                            RelationType = DXElementInUnitTypeEnum.SingleMandatory,
                            DXElementDefinitionUnit = blockDescObject.ID
                        }
                    }
                }
            };

            var item = new TestEntity()
            {
                ID = Guid.NewGuid(),
                TestBlock = new TestBlock()
                {
                    ID = Guid.NewGuid(),
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
                this._dataService.DeleteAsync(blockDescObject).Wait();
            };

            // Action
            this._dataService.InsertOrUpdateAsync(blockDescObject).Wait();
            this._dataService.InsertOrUpdateAsync(dxUnitDescObject).Wait();

            this._dataService.InsertOrUpdateAsync(item).Wait();

            // Assert
            var existingItems = this._genericRepo.GetDXUnits<TestEntity>();

            Assert.Single(existingItems);

            var existingItem = existingItems.Single();

            Assert.Equal(item.TestBlock.IntCln, existingItem.TestBlock.IntCln);

            // Action
            blockDescObject.DXColumnDefinitionElement = new DXMultiElementsContainer<DXColumnDefinitionElement>()
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

            this._dataService.InsertOrUpdateAsync(blockDescObject).Wait();

            // Assert
            var existingModifiedItems = this._genericRepo.GetDXUnits<TestEntityModified>();

            Assert.Single(existingItems);

            var existingItemModified = existingModifiedItems.Single();

            Assert.Equal("", existingItemModified.TestBlock.StrCln);
        }

        [Fact]
        public async Task GetItem_UsingMultiblockWithRelation_Ok()
        {
            // Init
            var id = new Guid("a03f744d-d5db-4d4e-95a8-d5fbf4bad2d7");

            // Action
            var jObject = await this._dataService.GetItemAsync("TDeviceUnit", id);

            // Assert
            Assert.Null(jObject["User"]);
        }

        [Fact]
        public async Task UpdateEntity_UsingAddedDeletedBlocksWithTargetMode_Ok()
        {
            // Init
            var id = new Guid("622c2056-9797-47ab-82c2-5c3eeb6a68ce");

            var blockToAdd = new DXElementDefinitionUnit()
            {
                ID = new Guid("4b95f498-f0cb-407d-be16-e7a1518fc070"),
                DXUnitDefinitionMainElement = new DXUnitDefinitionMainElement()
                {
                    ID = Guid.NewGuid(),
                    Name = "TestBlockToAdde7a1518fc070"
                }
            };

            var blockToDelete = new DXElementDefinitionUnit()
            {
                ID = new Guid("d3b5e1e2-3f3a-4f7c-8f0c-5e2b8e6f4a1c"),
                DXUnitDefinitionMainElement = new DXUnitDefinitionMainElement()
                {
                    ID = Guid.NewGuid(),
                    Name = "TestBlockToDelete5e2b8e6f4a1c"
                }
            };

            var dxUnit = new DXUnitDefinitionUnit()
            {
                ID = id,
                DXUnitDefinitionMainElement = new DXUnitDefinitionMainElement()
                {
                    ID = Guid.NewGuid(),
                    Name = "TestEntity5c3eeb6a68ce"
                }
            };

            base._finalizationAction = () =>
            {
                this._dataService.DeleteAsync(dxUnit).Wait();
                this._dataService.DeleteAsync(blockToAdd).Wait();
                this._dataService.DeleteAsync(blockToDelete).Wait();
            };

            this._dataService.InsertAsync(dxUnit).Wait();
            this._dataService.InsertAsync(blockToAdd).Wait();
            this._dataService.InsertAsync(blockToDelete).Wait();

            // Action
            dxUnit.DXElementInUnitDefinitionMainElement = new DXMultiElementsContainer<DXElementInUnitDefinitionMainElement>()
            {
                Mode = MultiElementsMode.Target,
                Announced = new List<DXElementInUnitDefinitionMainElement>()
                {
                    new DXElementInUnitDefinitionMainElement()
                    {
                        ID = Guid.NewGuid(),
                        RelationType = DXElementInUnitTypeEnum.SingleMandatory,
                        DXElementDefinitionUnit = blockToAdd.ID
                    }
                },
                Deleted = new List<DXElementInUnitDefinitionMainElement>()
                {
                    new DXElementInUnitDefinitionMainElement()
                    {
                        ID = Guid.NewGuid(),
                        RelationType = DXElementInUnitTypeEnum.SingleMandatory,
                        DXElementDefinitionUnit = blockToDelete.ID
                    }
                }
            };

            this._dataService.UpdateAsync(dxUnit).Wait();

            // Assert
            var existingEntity = await this._dataService.GetItemAsync<DXUnitDefinitionUnit>(id);

            Assert.Single(existingEntity.DXElementInUnitDefinitionMainElement.Announced);

            var announcedBlock = existingEntity.DXElementInUnitDefinitionMainElement.Announced.Single();

            Assert.Equal(blockToAdd.ID, announcedBlock.DXElementDefinitionUnit);

            // Action
            existingEntity.DXElementInUnitDefinitionMainElement = new DXMultiElementsContainer<DXElementInUnitDefinitionMainElement>()
            {
                Mode = MultiElementsMode.Target,
                Deleted = new List<DXElementInUnitDefinitionMainElement>()
                {
                    announcedBlock
                }
            };

            this._dataService.UpdateAsync(existingEntity).Wait();

            // Assert
            existingEntity = await this._dataService.GetItemAsync<DXUnitDefinitionUnit>(id);
            Assert.Empty(existingEntity.DXElementInUnitDefinitionMainElement.Announced);
        }


        [Fact]
        public async Task DeleteEntity_WithBlocks_Ok()
        {
            // Init
            var id = new Guid("1c4e8f3e-3f4b-4c6a-9f7e-8f9e7d6c5b4a");

            var block = new DXElementDefinitionUnit()
            {
                ID = new Guid("02449441-f8c4-483a-950f-6b47f2f216b4"),
                DXUnitDefinitionMainElement = new DXUnitDefinitionMainElement()
                {
                    ID = Guid.NewGuid(),
                    Name = "TestBlock6b47f2f216b4"
                }
            };

            var dxUnit = new DXUnitDefinitionUnit()
            {
                ID = id,
                DXUnitDefinitionMainElement = new DXUnitDefinitionMainElement()
                {
                    ID = Guid.NewGuid(),
                    Name = "TestEntity8f9e7d6c5b4a"
                },
                DXElementInUnitDefinitionMainElement = new DXMultiElementsContainer<DXElementInUnitDefinitionMainElement>()
                {
                    Announced = new List<DXElementInUnitDefinitionMainElement>()
                    {
                        new DXElementInUnitDefinitionMainElement()
                        {
                            ID = Guid.NewGuid(),
                            RelationType = DXElementInUnitTypeEnum.SingleMandatory,
                            DXElementDefinitionUnit = block.ID
                        }
                    }
                }
            };

            base._finalizationAction = () =>
            {
                this._dataService.DeleteAsync(dxUnit).Wait();
                this._dataService.DeleteAsync(block).Wait();
            };

            this._dataService.InsertAsync(block).Wait();
            this._dataService.InsertAsync(dxUnit).Wait();

            // Action
            this._dataService.DeleteAsync(dxUnit).Wait();

            // Assert
            var existingEntity = await this._dataService.GetItemAsync<DXUnitDefinitionUnit>(id);
            Assert.Null(existingEntity);

            var existingBlock = await this._dataService.GetItemAsync<DXElementDefinitionUnit>(block.ID);
            Assert.NotNull(existingBlock);
        }

        [Fact]
        public async Task UpdateEntity_UsingMoreThanOnelDeletedBlocksWithTargetMode_Ok()
        {
            // Init
            var id = new Guid("1873aa67-8f3e-4044-8659-c44f7a2dd5f6");

            var block1 = new DXElementDefinitionUnit()
            {
                ID = new Guid("f8f68404-6143-433e-aa2c-f45215f6be1c"),
                DXUnitDefinitionMainElement = new DXUnitDefinitionMainElement()
                {
                    ID = Guid.NewGuid(),
                    Name = "TestBlockf45215f6be1c"
                }
            };

            var block2 = new DXElementDefinitionUnit()
            {
                ID = new Guid("63c28d39-1561-4c97-b212-fa7db5443a11"),
                DXUnitDefinitionMainElement = new DXUnitDefinitionMainElement()
                {
                    ID = Guid.NewGuid(),
                    Name = "TestBlockfa7db5443a11"
                }
            };

            var block3 = new DXElementDefinitionUnit()
            {
                ID = new Guid("8ae093df-d6b8-4d13-acc7-801b464bfb0f"),
                DXUnitDefinitionMainElement = new DXUnitDefinitionMainElement()
                {
                    ID = Guid.NewGuid(),
                    Name = "TestBlock801b464bfb0f"
                }
            };

            var dxUnit = new DXUnitDefinitionUnit()
            {
                ID = id,
                DXUnitDefinitionMainElement = new DXUnitDefinitionMainElement()
                {
                    ID = Guid.NewGuid(),
                    Name = "TestEntityc44f7a2dd5f6"
                },
                DXElementInUnitDefinitionMainElement = new DXMultiElementsContainer<DXElementInUnitDefinitionMainElement>()
                {
                    Announced = new List<DXElementInUnitDefinitionMainElement>()
                    {
                        new DXElementInUnitDefinitionMainElement()
                        {
                            ID = Guid.NewGuid(),
                            RelationType = DXElementInUnitTypeEnum.SingleMandatory,
                            DXElementDefinitionUnit = block1.ID
                        },
                        new DXElementInUnitDefinitionMainElement()
                        {
                            ID = Guid.NewGuid(),
                            RelationType = DXElementInUnitTypeEnum.SingleMandatory,
                            DXElementDefinitionUnit = block2.ID
                        },
                        new DXElementInUnitDefinitionMainElement()
                        {
                            ID = Guid.NewGuid(),
                            RelationType = DXElementInUnitTypeEnum.SingleMandatory,
                            DXElementDefinitionUnit = block3.ID
                        }
                    }
                }
            };

            base._finalizationAction = () =>
            {
                this._dataService.DeleteAsync(dxUnit).Wait();
                this._dataService.DeleteAsync(block1).Wait();
                this._dataService.DeleteAsync(block2).Wait();
                this._dataService.DeleteAsync(block3).Wait();
            };

            this._dataService.InsertAsync(block1).Wait();
            this._dataService.InsertAsync(block2).Wait();
            this._dataService.InsertAsync(block3).Wait();
            this._dataService.InsertAsync(dxUnit).Wait();

            // Action
            dxUnit.DXElementInUnitDefinitionMainElement = new DXMultiElementsContainer<DXElementInUnitDefinitionMainElement>()
            {
                Mode = MultiElementsMode.Target,
                Deleted = dxUnit.DXElementInUnitDefinitionMainElement.Announced,
                Announced = new List<DXElementInUnitDefinitionMainElement>()
            };

            this._dataService.UpdateAsync(dxUnit).Wait();

            // Assert
            var existingEntity = await this._dataService.GetItemAsync<DXUnitDefinitionUnit>(id);
            Assert.Empty(existingEntity.DXElementInUnitDefinitionMainElement.Announced);
        }
    }

    [DXUnit("TestEntity")]
    public class TestEntity : DXUnit
    {
        public TestBlock TestBlock { get; set; }
    }

    [DXElement("TestBlock")]
    public class TestBlock : DXElement
    {
        [DXColumn("IntCln")]
        public int IntCln { get; set; }
    }

    [DXUnit("TestEntity")]
    public class TestEntityModified : DXUnit
    {
        public TestBlockModified TestBlock { get; set; }
    }

    [DXElement("TestBlock")]
    public class TestBlockModified : DXElement
    {
        [DXColumn("StrCln")]
        public string StrCln { get; set; }
    }
}