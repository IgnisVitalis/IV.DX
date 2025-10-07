using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.HandlerContext;
using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using IV.DX.Shared.IntTests;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace IV.DataProvider.Persistence.Services.IntTests.DataServiceTests
{
    public class DifferentCasesTests : IntTestController
    {
        IDataService _dataService;
        IGenericRepository _genericRepo;

        public DifferentCasesTests(ITestOutputHelper output)
            : base(output)
        {
            this._dataService = this.ServiceProvider.GetService<IDataService>();
            this._genericRepo = this.ServiceProvider.GetService<IGenericRepository>();
        }

        [Fact]
        public void UpdateEntityDefinition_UsingTargetModeForColumnDefinitionWithEmptyDefinitions_Ok()
        {
            // Init
            var id = new Guid("f0ff00d1-303e-42e6-9769-e482b0bf79ff");

            var intCln = new DPColumnDescBlock()
            {
                ID = Guid.NewGuid(),
                Name = "IntCln",
                ColumnType = DPColumnTypeEnum.Int
            };

            var strCln = new DPColumnDescBlock()
            {
                ID = Guid.NewGuid(),
                Name = "StrCln",
                Length = 100,
                DefaultValue = "''",
                ColumnType = DPColumnTypeEnum.String
            };

            DXElementDefinitionUnit blockDescObject = new DXElementDefinitionUnit()
            {
                ID = id,
                DPObjectDescGenBlock = new DPObjectDescGenBlock()
                {
                    ID = Guid.NewGuid(),
                    Name = "TestBlock"
                },
                DPColumnDescBlock = new ESQLMultiItemsContainer<DPColumnDescBlock>()
                {
                    Mode = ModeForMultiItems.Target,
                    Announced = new List<DPColumnDescBlock>()
                    {
                        intCln
                    }
                }
            };

            DXUnitDefinitionUnit entityDescObject = new DXUnitDefinitionUnit()
            {
                ID = Guid.NewGuid(),
                DPObjectDescGenBlock = new DPObjectDescGenBlock()
                {
                    ID = Guid.NewGuid(),
                    Name = "TestEntity"
                },
                DPBlockInEntityDescGenBlock = new ESQLMultiItemsContainer<DPBlockInEntityDescGenBlock>()
                {
                    Announced = new List<DPBlockInEntityDescGenBlock>()
                    {
                        new DPBlockInEntityDescGenBlock()
                        {
                            ID = Guid.NewGuid(),
                            RelationType = DPBlockInObjectTypeEnum.SingleMandatory,
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
                    this._dataService.Delete(item);
                });

                this._dataService.Update(entityDescObject);
                this._dataService.Delete(entityDescObject);
                this._dataService.Delete(blockDescObject);
            };

            // Action
            this._dataService.InsertOrUpdate(blockDescObject);
            this._dataService.InsertOrUpdate(entityDescObject);

            this._dataService.InsertOrUpdate(item);

            // Assert
            var existingItems = this._genericRepo.GetItems<TestEntity>();

            Assert.Single(existingItems);

            var existingItem = existingItems.Single();

            Assert.Equal(item.TestBlock.IntCln, existingItem.TestBlock.IntCln);

            // Action
            blockDescObject.DPColumnDescBlock = new ESQLMultiItemsContainer<DPColumnDescBlock>()
            {
                Mode = ModeForMultiItems.Target,
                Announced = new List<DPColumnDescBlock>()
                {
                    strCln
                },
                Deleted = new List<DPColumnDescBlock>()
                {
                    intCln
                }
            };

            this._dataService.InsertOrUpdate(blockDescObject);

            // Assert
            var existingModifiedItems = this._genericRepo.GetItems<TestEntityModified>();

            Assert.Single(existingItems);

            var existingItemModified = existingModifiedItems.Single();

            Assert.Equal("", existingItemModified.TestBlock.StrCln);
        }

        [Fact]
        public void GetItem_UsingMultiblockWithRelation_Ok()
        {
            // Init
            var id = new Guid("a03f744d-d5db-4d4e-95a8-d5fbf4bad2d7");

            // Action
            var model = this._dataService.GetItem("TDeviceObject", id, new EntityHandlerBaseContext());

            // Assert
            var jObject = model.ConvertToJObject();

            Assert.Null(jObject["User"]);
        }

        [Fact]
        public void UpdateEntity_UsingAddedDeletedBlocksWithTargetMode_Ok()
        {
            // Init
            var id = new Guid("622c2056-9797-47ab-82c2-5c3eeb6a68ce");

            var blockToAdd = new DXElementDefinitionUnit()
            {
                ID = new Guid("4b95f498-f0cb-407d-be16-e7a1518fc070"),
                DPObjectDescGenBlock = new DPObjectDescGenBlock()
                {
                    ID = Guid.NewGuid(),
                    Name = "TestBlockToAdde7a1518fc070"
                }
            };

            var blockToDelete = new DXElementDefinitionUnit()
            {
                ID = new Guid("d3b5e1e2-3f3a-4f7c-8f0c-5e2b8e6f4a1c"),
                DPObjectDescGenBlock = new DPObjectDescGenBlock()
                {
                    ID = Guid.NewGuid(),
                    Name = "TestBlockToDelete5e2b8e6f4a1c"
                }
            };

            var entity = new DXUnitDefinitionUnit()
            {
                ID = id,
                DPObjectDescGenBlock = new DPObjectDescGenBlock()
                {
                    ID = Guid.NewGuid(),
                    Name = "TestEntity5c3eeb6a68ce"
                }
            };

            base._finalizationAction = () =>
            {
                this._dataService.Delete(entity);
                this._dataService.Delete(blockToAdd);
                this._dataService.Delete(blockToDelete);
            };

            this._dataService.Insert(entity);
            this._dataService.Insert(blockToAdd);
            this._dataService.Insert(blockToDelete);

            // Action
            entity.DPBlockInEntityDescGenBlock = new ESQLMultiItemsContainer<DPBlockInEntityDescGenBlock>()
            {
                Mode = ModeForMultiItems.Target,
                Announced = new List<DPBlockInEntityDescGenBlock>()
                {
                    new DPBlockInEntityDescGenBlock()
                    {
                        ID = Guid.NewGuid(),
                        RelationType = DPBlockInObjectTypeEnum.SingleMandatory,
                        DXElementDefinitionUnit = blockToAdd.ID
                    }
                },
                Deleted = new List<DPBlockInEntityDescGenBlock>()
                {
                    new DPBlockInEntityDescGenBlock()
                    {
                        ID = Guid.NewGuid(),
                        RelationType = DPBlockInObjectTypeEnum.SingleMandatory,
                        DXElementDefinitionUnit = blockToDelete.ID
                    }
                }
            };

            this._dataService.Update(entity);

            // Assert
            var existingEntity = this._dataService.GetItem<DXUnitDefinitionUnit>(id);

            Assert.Single(existingEntity.DPBlockInEntityDescGenBlock.Announced);

            var announcedBlock = existingEntity.DPBlockInEntityDescGenBlock.Announced.Single();

            Assert.Equal(blockToAdd.ID, announcedBlock.DXElementDefinitionUnit);

            // Action
            existingEntity.DPBlockInEntityDescGenBlock = new ESQLMultiItemsContainer<DPBlockInEntityDescGenBlock>()
            {
                Mode = ModeForMultiItems.Target,
                Deleted = new List<DPBlockInEntityDescGenBlock>()
                {
                    announcedBlock
                }
            };

            this._dataService.Update(existingEntity);

            // Assert
            existingEntity = this._dataService.GetItem<DXUnitDefinitionUnit>(id);
            Assert.Empty(existingEntity.DPBlockInEntityDescGenBlock.Announced);
        }


        [Fact]
        public void DeleteEntity_WithBlocks_Ok()
        {
            // Init
            var id = new Guid("1c4e8f3e-3f4b-4c6a-9f7e-8f9e7d6c5b4a");

            var block = new DXElementDefinitionUnit()
            {
                ID = new Guid("02449441-f8c4-483a-950f-6b47f2f216b4"),
                DPObjectDescGenBlock = new DPObjectDescGenBlock()
                {
                    ID = Guid.NewGuid(),
                    Name = "TestBlock6b47f2f216b4"
                }
            };

            var entity = new DXUnitDefinitionUnit()
            {
                ID = id,
                DPObjectDescGenBlock = new DPObjectDescGenBlock()
                {
                    ID = Guid.NewGuid(),
                    Name = "TestEntity8f9e7d6c5b4a"
                },
                DPBlockInEntityDescGenBlock = new ESQLMultiItemsContainer<DPBlockInEntityDescGenBlock>()
                {
                    Announced = new List<DPBlockInEntityDescGenBlock>()
                    {
                        new DPBlockInEntityDescGenBlock()
                        {
                            ID = Guid.NewGuid(),
                            RelationType = DPBlockInObjectTypeEnum.SingleMandatory,
                            DXElementDefinitionUnit = block.ID
                        }
                    }
                }
            };

            base._finalizationAction = () =>
            {
                this._dataService.Delete(entity);
                this._dataService.Delete(block);
            };

            this._dataService.Insert(block);
            this._dataService.Insert(entity);

            // Action
            this._dataService.Delete(entity);

            // Assert
            var existingEntity = this._dataService.GetItem<DXUnitDefinitionUnit>(id);
            Assert.Null(existingEntity);

            var existingBlock = this._dataService.GetItem<DXElementDefinitionUnit>(block.ID);
            Assert.NotNull(existingBlock);
        }

        [Fact]
        public void UpdateEntity_UsingMoreThanOnelDeletedBlocksWithTargetMode_Ok()
        {
            // Init
            var id = new Guid("1873aa67-8f3e-4044-8659-c44f7a2dd5f6");

            var block1 = new DXElementDefinitionUnit()
            {
                ID = new Guid("f8f68404-6143-433e-aa2c-f45215f6be1c"),
                DPObjectDescGenBlock = new DPObjectDescGenBlock()
                {
                    ID = Guid.NewGuid(),
                    Name = "TestBlockf45215f6be1c"
                }
            };

            var block2 = new DXElementDefinitionUnit()
            {
                ID = new Guid("63c28d39-1561-4c97-b212-fa7db5443a11"),
                DPObjectDescGenBlock = new DPObjectDescGenBlock()
                {
                    ID = Guid.NewGuid(),
                    Name = "TestBlockfa7db5443a11"
                }
            };

            var block3 = new DXElementDefinitionUnit()
            {
                ID = new Guid("8ae093df-d6b8-4d13-acc7-801b464bfb0f"),
                DPObjectDescGenBlock = new DPObjectDescGenBlock()
                {
                    ID = Guid.NewGuid(),
                    Name = "TestBlock801b464bfb0f"
                }
            };

            var entity = new DXUnitDefinitionUnit()
            {
                ID = id,
                DPObjectDescGenBlock = new DPObjectDescGenBlock()
                {
                    ID = Guid.NewGuid(),
                    Name = "TestEntityc44f7a2dd5f6"
                },
                DPBlockInEntityDescGenBlock = new ESQLMultiItemsContainer<DPBlockInEntityDescGenBlock>()
                {
                    Announced = new List<DPBlockInEntityDescGenBlock>()
                    {
                        new DPBlockInEntityDescGenBlock()
                        {
                            ID = Guid.NewGuid(),
                            RelationType = DPBlockInObjectTypeEnum.SingleMandatory,
                            DXElementDefinitionUnit = block1.ID
                        },
                        new DPBlockInEntityDescGenBlock()
                        {
                            ID = Guid.NewGuid(),
                            RelationType = DPBlockInObjectTypeEnum.SingleMandatory,
                            DXElementDefinitionUnit = block2.ID
                        },
                        new DPBlockInEntityDescGenBlock()
                        {
                            ID = Guid.NewGuid(),
                            RelationType = DPBlockInObjectTypeEnum.SingleMandatory,
                            DXElementDefinitionUnit = block3.ID
                        }
                    }
                }
            };

            base._finalizationAction = () =>
            {
                this._dataService.Delete(entity);
                this._dataService.Delete(block1);
                this._dataService.Delete(block2);
                this._dataService.Delete(block3);
            };

            this._dataService.Insert(block1);
            this._dataService.Insert(block2);
            this._dataService.Insert(block3);
            this._dataService.Insert(entity);

            // Action
            entity.DPBlockInEntityDescGenBlock = new ESQLMultiItemsContainer<DPBlockInEntityDescGenBlock>()
            {
                Mode = ModeForMultiItems.Target,               
                Deleted = entity.DPBlockInEntityDescGenBlock.Announced,
                Announced = new List<DPBlockInEntityDescGenBlock>()
            };

            this._dataService.Update(entity);

            // Assert
            var existingEntity = this._dataService.GetItem<DXUnitDefinitionUnit>(id);
            Assert.Empty(existingEntity.DPBlockInEntityDescGenBlock.Announced);
        }
    }

    [ESQLObjectDefinition("TestEntity")]
    public class TestEntity : ESQLObject
    {
        public TestBlock TestBlock { get; set; }
    }

    [ESQLBlockDefinition("TestBlock")]
    public class TestBlock : ESQLBlock
    {
        [ESQLColumnDefinition("IntCln")]
        public int IntCln { get; set; }
    }

    [ESQLObjectDefinition("TestEntity")]
    public class TestEntityModified : ESQLObject
    {
        public TestBlockModified TestBlock { get; set; }
    }

    [ESQLBlockDefinition("TestBlock")]
    public class TestBlockModified : ESQLBlock
    {
        [ESQLColumnDefinition("StrCln")]
        public string StrCln { get; set; }
    }
}