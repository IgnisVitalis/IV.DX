using IV.DataProvider.Persistence.Shared.IntTests;
using IV.DataProvider.Persistence.Shared.IntTests.Factories;
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
using ObjectFactory = IV.DataProvider.Persistence.Shared.IntTests.Factories.ObjectFactory;

namespace IV.DataProvider.Persistence.Repositories.IntTests
{
    public class RelationRepositoryTests : IntTestController
    {
        IGenericRepository _genericRepo;
        ICoreRepository _coreRepo;

        public RelationRepositoryTests(ITestOutputHelper output)
            : base(output)
        {
            this._genericRepo = this.ServiceProvider.GetService<IGenericRepository>();
            this._coreRepo = this.ServiceProvider.GetService<ICoreRepository>();
        }

        [Fact]
        public void CRUDRelations_UsingManyToManyRelation_Success()
        {
            // Init               
            var obj1Id = new Guid("99e96462-81c3-46d2-b38b-6f0e158d060a");
            var obj2Id = new Guid("a0aa9ec4-8b67-47ac-8c5c-107c01f839b5");
            var objRelId = new Guid("c2f2f201-bbc6-4c5e-b7f2-70805420911b");

            var obj1 = ObjectFactory.GetItem(obj1Id, "RelTableLeft7");

            var obj2 = ObjectFactory.GetItem(obj2Id, "RelTableRight7");

            DXRelationDefinitionUnit objRelationInfo =
                DXRelationDefinitionUnitFactory.GetItem(objRelId,
                DXRelationTypeEnum.ManyToMany,
                "RelTableLeftRelation",
                "RelTableRightRelation",
                obj1,
                obj2);

            var relTablesLeft = Enumerable.Range(0, 5).Select(x => new RelTableLeft7() { ID = Guid.NewGuid() }).ToList();
            var relTablesRight = Enumerable.Range(0, 5).Select(x => new RelTableRight7() { ID = Guid.NewGuid() }).ToList();

            var relTableFirstLeft = relTablesLeft.First();
            var relTableFirstRight = relTablesRight.First();

            base._finalizationAction = new Action(() =>
            {
                this._dataService.Delete(objRelationInfo);
                this._dataService.Delete(obj1);
                this._dataService.Delete(obj2);
            });

            this._dataService.Insert(obj1);
            this._dataService.Insert(obj2);

            // Action           
            this._dataService.Insert(objRelationInfo);

            // Checking
            var createdRelationEntry = this._genericRepo.GetItem<DXRelationDefinitionUnit>(objRelationInfo.ID);
            var createdInvertedRelationEntry = createdRelationEntry.CreateInvertedRelationObject();

            this.CheckDXRelationDefinitionUnit(objRelationInfo, createdRelationEntry);
            Assert.Equal("Relation_RelTableLeft7_RelTableRight7_0", createdRelationEntry.DXRelationDefinitionMainElement.RelationTable);

            this.CheckInvertedRelationEntry(createdRelationEntry, createdInvertedRelationEntry);

            // Init
            foreach (var item in relTablesLeft)
            {
                this._genericRepo.Insert(item);
            }

            foreach (var item in relTablesRight)
            {
                this._genericRepo.Insert(item);
            }

            // Checking relations between items
            this.CheckingMethodsForRelations(objRelationInfo, obj1, obj2, relTableFirstLeft, relTableFirstRight);
        }


        [Fact]
        public void CRUDRelations_UsingManyToOneRelation_Success()
        {
            // Init
            var obj1Id = new Guid("36bd2e27-5c8f-4ee0-bf3f-9b5334da0c7a");
            var obj2Id = new Guid("b541f19e-af85-4e31-9e5e-349a47b82d62");
            var objRelId = new Guid("477e10dc-a6ed-49a5-9b0f-3a85024e9e67");

            var obj1 = ObjectFactory.GetItem(obj1Id, "RelTableLeft4");

            var obj2 = ObjectFactory.GetItem(obj2Id, "RelTableRight4");

            DXRelationDefinitionUnit objRelationInfo =
                DXRelationDefinitionUnitFactory.GetItem(objRelId,
                DXRelationTypeEnum.ManyToOne,
                "RelTableLeftRelation",
                "RelTableRightRelation",
                obj1,
                obj2);

            var relTablesLeft = Enumerable.Range(0, 5).Select(x => new RelTableLeft4() { ID = Guid.NewGuid() }).ToList();
            var relTablesRight = Enumerable.Range(0, 5).Select(x => new RelTableRight4() { ID = Guid.NewGuid() }).ToList();

            var relTableFirstLeft = relTablesLeft.First();
            var relTableFirstRight = relTablesRight.First();
            var relTableFifthRight = relTablesRight.Skip(4).First();

            base._finalizationAction = new Action(() =>
            {
                this._dataService.Delete(objRelationInfo);
                this._dataService.Delete(obj1);
                this._dataService.Delete(obj2);
            });

            this._dataService.Insert(obj1);
            this._dataService.Insert(obj2);

            // Action        
            this._dataService.Insert(objRelationInfo);

            // Checking
            var createdRelationEntry = this._genericRepo.GetItem<DXRelationDefinitionUnit>(objRelationInfo.ID);
            var createdInvertedRelationEntry = createdRelationEntry.CreateInvertedRelationObject();

            this.CheckDXRelationDefinitionUnit(objRelationInfo, createdRelationEntry);
            Assert.Null(createdRelationEntry.DXRelationDefinitionMainElement.RelationTable);

            this.CheckInvertedRelationEntry(createdRelationEntry, createdInvertedRelationEntry);

            // Init
            foreach (var item in relTablesRight)
            {
                this._genericRepo.Insert(item);
            }

            foreach (var item in relTablesLeft)
            {
                item.RelTableRightRelation = relTableFifthRight.ID;
                this._genericRepo.Insert(item);
            }

            // Checking relations between items
            this.CheckingMethodsForRelations(objRelationInfo, obj1, obj2, relTableFirstLeft, relTableFirstRight);
        }

        [Fact]
        public void CRUDRelations_UsingOneToManyRelation_Success()
        {
            // Init
            var obj1Id = new Guid("c05afb4a-d2ed-42c9-b9fc-4310461cfa99");
            var obj2Id = new Guid("a02f0ea1-3dbc-41b8-a545-cccad5e29534");
            var objRelId = new Guid("68ae16b3-27b2-43f7-996f-a5ac51340944");

            var obj1 = ObjectFactory.GetItem(obj1Id, "RelTableLeft3");

            var obj2 = ObjectFactory.GetItem(obj2Id, "RelTableRight3");

            DXRelationDefinitionUnit objRelationInfo =
                DXRelationDefinitionUnitFactory.GetItem(objRelId,
                DXRelationTypeEnum.OneToMany,
                "RelTableLeftRelation",
                "RelTableRightRelation",
                obj1,
                obj2);

            var relTablesLeft = Enumerable.Range(0, 5).Select(x => new RelTableLeft3() { ID = Guid.NewGuid() }).ToList();
            var relTablesRight = Enumerable.Range(0, 5).Select(x => new RelTableRight3() { ID = Guid.NewGuid() }).ToList();

            var relTableFirstLeft = relTablesLeft.First();
            var relTableFifthLeft = relTablesLeft.Skip(4).First();
            var relTableFirstRight = relTablesRight.First();

            base._finalizationAction = new Action(() =>
            {
                this._dataService.Delete(objRelationInfo);
                this._dataService.Delete(obj1);
                this._dataService.Delete(obj2);
            });

            this._dataService.Insert(obj1);
            this._dataService.Insert(obj2);

            // Action        
            this._dataService.Insert(objRelationInfo);

            // Checking
            var createdRelationEntry = this._genericRepo.GetItem<DXRelationDefinitionUnit>(objRelationInfo.ID);
            var createdInvertedRelationEntry = createdRelationEntry.CreateInvertedRelationObject();

            this.CheckDXRelationDefinitionUnit(objRelationInfo, createdRelationEntry);
            Assert.Null(createdRelationEntry.DXRelationDefinitionMainElement.RelationTable);

            this.CheckInvertedRelationEntry(createdRelationEntry, createdInvertedRelationEntry);

            // Init
            foreach (var item in relTablesLeft)
            {
                this._genericRepo.Insert(item);
            }

            foreach (var item in relTablesRight)
            {
                item.RelTableLeftRelation = relTableFifthLeft.ID;
                this._genericRepo.Insert(item);
            }

            // Checking relations between items
            this.CheckingMethodsForRelations(objRelationInfo, obj1, obj2, relTableFirstLeft, relTableFirstRight);
        }

        [Fact]
        public void CRUDRelations_UsingOneToZeroOneRelation_Success()
        {
            // Init
            var obj1Id = new Guid("92c50be6-43a1-43c3-b936-500e9c9dce2e");
            var obj2Id = new Guid("a3e72030-6892-4875-84c6-af18151312d3");
            var objRelId = new Guid("73b722a2-119e-4c4a-8b62-060ea45625ce");

            DPObjectDescObject obj1 = ObjectFactory.GetItem(obj1Id, "RelTableLeft43a1");
            DPObjectDescObject obj2 = ObjectFactory.GetItem(obj2Id, "RelTableRight43a1");

            DXRelationDefinitionUnit objRelationInfo =
                DXRelationDefinitionUnitFactory.GetItem(objRelId,
                DXRelationTypeEnum.OneToZeroOne,
                "RelTableLeftRelation",
                "RelTableRightRelation",
                obj1,
                obj2);

            int entriesCount = 5;
            var relTablesLeft = Enumerable.Range(0, entriesCount).Select(x => new RelTableLeft1() { ID = Guid.NewGuid() }).ToList();
            var relTablesRight = Enumerable.Range(0, entriesCount).Select(x => new RelTableRight1() { ID = Guid.NewGuid() }).ToList();

            var relTableFirstLeft = relTablesLeft.First();
            var relTableFirstRight = relTablesRight.First();

            base._finalizationAction = new Action(() =>
            {
                this._dataService.Delete(objRelationInfo);
                this._dataService.Delete(obj1);
                this._dataService.Delete(obj2);
            });

            this._dataService.Insert(obj1);
            this._dataService.Insert(obj2);

            // Action        
            this._dataService.Insert(objRelationInfo);

            // Checking
            var createdRelationEntry = this._genericRepo.GetItem<DXRelationDefinitionUnit>(objRelationInfo.ID);
            var createdInvertedRelationEntry = createdRelationEntry.CreateInvertedRelationObject();

            this.CheckDXRelationDefinitionUnit(objRelationInfo, createdRelationEntry);
            Assert.Null(createdRelationEntry.DXRelationDefinitionMainElement.RelationTable);

            this.CheckInvertedRelationEntry(createdRelationEntry, createdInvertedRelationEntry);

            // Init
            foreach (var item in relTablesLeft)
            {
                this._genericRepo.Insert(item);
            }

            for (int i = 0; i < entriesCount; i++)
            {
                var itemLeft = relTablesLeft.Skip(i).First();
                var itemRight = relTablesRight.Skip(i).First();

                itemRight.RelTableLeftRelation = itemLeft.ID;

                this._genericRepo.Insert(itemRight);
            }

            // Checking relations between items
            this.CheckingMethodsForRelations(objRelationInfo, obj1, obj2, relTableFirstLeft, relTableFirstRight);

            // Checking restricted actions
            Action action = () =>
            {
                var itemLeft = relTablesLeft.Skip(1).First();
                var itemRight = relTablesRight.Skip(2).First();

                this._coreRepo.AddRelation(obj1.DXUnitDefinitionMainElement.Name, itemLeft.ID, "RelTableRightRelation", obj2.DXUnitDefinitionMainElement.Name, itemRight.ID);
            };

            for (int i = 0; i < entriesCount; i++)
            {
                Assert.Throws<Exception>(() =>
                {
                    try
                    {
                        var itemLeft = relTablesLeft.Skip(i).First();
                        var itemRight = relTablesRight.Skip((i + 1) % entriesCount).First();

                        this._coreRepo.AddRelation(obj1.DXUnitDefinitionMainElement.Name, itemLeft.ID, "RelTableRightRelation", obj2.DXUnitDefinitionMainElement.Name, itemRight.ID);
                    }
                    catch (Exception exc)
                    {
                        throw new Exception(exc.Message);
                    }
                });
            }

            for (int i = 0; i < entriesCount; i++)
            {
                Assert.Throws<Exception>(() =>
                {
                    try
                    {
                        var itemLeft = relTablesLeft.Skip(i).First();
                        var itemRight = relTablesRight.Skip((i + 1) % entriesCount).First();

                        this._coreRepo.AddRelation(obj2.DXUnitDefinitionMainElement.Name, itemRight.ID, "RelTableLeftRelation", obj1.DXUnitDefinitionMainElement.Name, itemLeft.ID);
                    }
                    catch (Exception exc)
                    {
                        throw new Exception(exc.Message);
                    }
                });
            }

            for (int i = 0; i < entriesCount; i++)
            {
                Assert.Throws<Exception>(() =>
                {
                    try
                    {
                        var itemLeft = relTablesLeft.Skip(i).First();
                        var itemRight = relTablesRight.Skip(i).First();

                        this._coreRepo.RemoveRelation(obj1.DXUnitDefinitionMainElement.Name, itemLeft.ID, "RelTableRightRelation", obj2.DXUnitDefinitionMainElement.Name, itemRight.ID);
                    }
                    catch (Exception exc)
                    {
                        throw new Exception(exc.Message);
                    }
                });
            }

            for (int i = 0; i < entriesCount; i++)
            {
                Assert.Throws<Exception>(() =>
                {
                    try
                    {
                        var itemLeft = relTablesLeft.Skip(i).First();
                        var itemRight = relTablesRight.Skip(i).First();

                        this._coreRepo.RemoveRelation(obj2.DXUnitDefinitionMainElement.Name, itemRight.ID, "RelTableLeftRelation", obj1.DXUnitDefinitionMainElement.Name, itemLeft.ID);
                    }
                    catch (Exception exc)
                    {
                        throw new Exception(exc.Message);
                    }
                });
            }
        }

        [Fact]
        public void CRUDRelations_UsingZeroOneToOneRelation_Success()
        {
            // Init
            var obj1Id = new Guid("f7de0979-8984-48eb-ac0a-98a1e94ca0e3");
            var obj2Id = new Guid("68d522fe-eed4-4d41-a0f9-8dfe68352a41");
            var objRelId = new Guid("b18ec4f1-43d4-4e5d-9c1e-298b50d7e0ff");

            var obj1 = ObjectFactory.GetItem(obj1Id, "RelTableLeft2");

            var obj2 = ObjectFactory.GetItem(obj2Id, "RelTableRight2");

            DXRelationDefinitionUnit objRelationInfo =
                DXRelationDefinitionUnitFactory.GetItem(objRelId,
                DXRelationTypeEnum.ZeroOneToOne,
                "RelTableLeftRelation",
                "RelTableRightRelation",
                obj1,
                obj2);

            int entriesCount = 5;
            var relTablesLeft = Enumerable.Range(0, entriesCount).Select(x => new RelTableLeft2() { ID = Guid.NewGuid() }).ToList();
            var relTablesRight = Enumerable.Range(0, entriesCount).Select(x => new RelTableRight2() { ID = Guid.NewGuid() }).ToList();

            var relTableFirstLeft = relTablesLeft.First();
            var relTableFirstRight = relTablesRight.First();

            base._finalizationAction = new Action(() =>
            {
                this._dataService.Delete(objRelationInfo);
                this._dataService.Delete(obj1);
                this._dataService.Delete(obj2);
            });

            this._dataService.Insert(obj1);
            this._dataService.Insert(obj2);

            // Action        
            this._dataService.Insert(objRelationInfo);

            // Checking
            var createdRelationEntry = this._genericRepo.GetItem<DXRelationDefinitionUnit>(objRelationInfo.ID);
            var createdInvertedRelationEntry = createdRelationEntry.CreateInvertedRelationObject();

            this.CheckDXRelationDefinitionUnit(objRelationInfo, createdRelationEntry);
            Assert.Null(createdRelationEntry.DXRelationDefinitionMainElement.RelationTable);

            this.CheckInvertedRelationEntry(createdRelationEntry, createdInvertedRelationEntry);

            // Init
            foreach (var item in relTablesRight)
            {
                this._genericRepo.Insert(item);
            }

            for (int i = 0; i < entriesCount; i++)
            {
                var itemLeft = relTablesLeft.Skip(i).First();
                var itemRight = relTablesRight.Skip(i).First();

                itemLeft.RelTableRightRelation = itemRight.ID;

                this._genericRepo.Insert(itemLeft);
            }

            // Checking relations between items
            this.CheckingMethodsForRelations(objRelationInfo, obj1, obj2, relTableFirstLeft, relTableFirstRight);

            // Checking restricted actions
            Action action = () =>
            {
                var itemLeft = relTablesLeft.Skip(1).First();
                var itemRight = relTablesRight.Skip(2).First();

                this._coreRepo.AddRelation(obj1.DXUnitDefinitionMainElement.Name, itemLeft.ID, "RelTableRightRelation", obj2.DXUnitDefinitionMainElement.Name, itemRight.ID);
            };

            for (int i = 0; i < entriesCount; i++)
            {
                Assert.Throws<Exception>(() =>
                {
                    try
                    {
                        var itemLeft = relTablesLeft.Skip(i).First();
                        var itemRight = relTablesRight.Skip((i + 1) % entriesCount).First();

                        this._coreRepo.AddRelation(obj1.DXUnitDefinitionMainElement.Name, itemLeft.ID, "RelTableRightRelation", obj2.DXUnitDefinitionMainElement.Name, itemRight.ID);
                    }
                    catch (Exception exc)
                    {
                        throw new Exception(exc.Message);
                    }
                });
            }

            for (int i = 0; i < entriesCount; i++)
            {
                Assert.Throws<Exception>(() =>
                {
                    try
                    {
                        var itemLeft = relTablesLeft.Skip(i).First();
                        var itemRight = relTablesRight.Skip((i + 1) % entriesCount).First();

                        this._coreRepo.AddRelation(obj2.DXUnitDefinitionMainElement.Name, itemRight.ID, "RelTableLeftRelation", obj1.DXUnitDefinitionMainElement.Name, itemLeft.ID);
                    }
                    catch (Exception exc)
                    {
                        throw new Exception(exc.Message);
                    }
                });
            }

            for (int i = 0; i < entriesCount; i++)
            {
                Assert.Throws<Exception>(() =>
                {
                    try
                    {
                        var itemLeft = relTablesLeft.Skip(i).First();
                        var itemRight = relTablesRight.Skip(i).First();

                        this._coreRepo.RemoveRelation(obj1.DXUnitDefinitionMainElement.Name, itemLeft.ID, "RelTableRightRelation", obj2.DXUnitDefinitionMainElement.Name, itemRight.ID);
                    }
                    catch (Exception exc)
                    {
                        throw new Exception(exc.Message);
                    }
                });
            }

            for (int i = 0; i < entriesCount; i++)
            {
                Assert.Throws<Exception>(() =>
                {
                    try
                    {
                        var itemLeft = relTablesLeft.Skip(i).First();
                        var itemRight = relTablesRight.Skip(i).First();

                        this._coreRepo.RemoveRelation(obj2.DXUnitDefinitionMainElement.Name, itemRight.ID, "RelTableLeftRelation", obj1.DXUnitDefinitionMainElement.Name, itemLeft.ID);
                    }
                    catch (Exception exc)
                    {
                        throw new Exception(exc.Message);
                    }
                });
            }
        }

        [Fact]
        public void CRUDRelations_UsingManyToZeroOneRelation_Success()
        {
            // Init
            var obj1Id = new Guid("e8e313ad-0b4b-42c2-a076-5af52c41d44a");
            var obj2Id = new Guid("5ed2db92-624e-4f5c-b2d0-82f33511d970");
            var objRelId = new Guid("3923b374-5387-4ef3-a0fd-807e2f69b0b7");

            var obj1 = ObjectFactory.GetItem(obj1Id, "RelTableLeft6");

            var obj2 = ObjectFactory.GetItem(obj2Id, "RelTableRight6");

            DXRelationDefinitionUnit objRelationInfo =
                DXRelationDefinitionUnitFactory.GetItem(objRelId,
                DXRelationTypeEnum.ManyToZeroOne,
                "RelTableLeftRelation",
                "RelTableRightRelation",
                obj1,
                obj2);

            int entriesCount = 5;
            var relTablesLeft = Enumerable.Range(0, entriesCount).Select(x => new RelTableLeft6() { ID = Guid.NewGuid() }).ToList();
            var relTablesRight = Enumerable.Range(0, entriesCount).Select(x => new RelTableRight6() { ID = Guid.NewGuid() }).ToList();

            var relTableFirstLeft = relTablesLeft.First();
            var relTableFirstRight = relTablesRight.First();

            base._finalizationAction = new Action(() =>
            {
                this._dataService.Delete(objRelationInfo);
                this._dataService.Delete(obj1);
                this._dataService.Delete(obj2);
            });

            this._dataService.Insert(obj1);
            this._dataService.Insert(obj2);

            // Action        
            this._dataService.Insert(objRelationInfo);

            // Checking
            var createdRelationEntry = this._genericRepo.GetItem<DXRelationDefinitionUnit>(objRelationInfo.ID);
            var createdInvertedRelationEntry = createdRelationEntry.CreateInvertedRelationObject();

            this.CheckDXRelationDefinitionUnit(objRelationInfo, createdRelationEntry);
            Assert.Null(createdRelationEntry.DXRelationDefinitionMainElement.RelationTable);

            this.CheckInvertedRelationEntry(createdRelationEntry, createdInvertedRelationEntry);

            // Init
            foreach (var item in relTablesLeft)
            {
                this._genericRepo.Insert(item);
            }

            foreach (var item in relTablesRight)
            {
                this._genericRepo.Insert(item);
            }

            // Checking relations between items
            var relTablesLeftCreated = this._genericRepo.GetItems<RelTableLeft6>(relTablesLeft.Select(x => x.ID));

            Assert.Equal(entriesCount, relTablesLeftCreated.Count());
            Assert.Equal(entriesCount, relTablesLeftCreated.Select(x => x.ID).Intersect(relTablesLeft.Select(x => x.ID)).Count());

            foreach (var item in relTablesLeftCreated)
            {
                Assert.Null(item.RelTableRightRelation);
            }

            this.CheckingMethodsForRelations(objRelationInfo, obj1, obj2, relTableFirstLeft, relTableFirstRight);

            // Additional checking
            foreach (var item in relTablesLeft)
            {
                this._coreRepo.AddRelation(obj1.DXUnitDefinitionMainElement.Name, item.ID, "RelTableRightRelation", obj2.DXUnitDefinitionMainElement.Name, relTableFirstRight.ID);
            }

            foreach (var item in relTablesLeft)
            {
                var relationRightID = this._coreRepo.GetRelation(obj1.DXUnitDefinitionMainElement.Name, item.ID, "RelTableRightRelation");
                var relationRightIDs = this._coreRepo.GetRelations(obj1.DXUnitDefinitionMainElement.Name, item.ID, "RelTableRightRelation");

                Assert.Equal(relTableFirstRight.ID, relationRightID);
                Assert.Single(relationRightIDs);
                Assert.Equal(relTableFirstRight.ID, relationRightIDs.Single());
            }

            relTablesLeftCreated = this._genericRepo.GetItems<RelTableLeft6>(relTablesLeft.Select(x => x.ID));

            foreach (var item in relTablesLeftCreated)
            {
                Assert.Equal(relTableFirstRight.ID, item.RelTableRightRelation);
            }

            var relationLeftIDs = this._coreRepo.GetRelations(obj2.DXUnitDefinitionMainElement.Name, relTableFirstRight.ID, "RelTableLeftRelation");

            Assert.Equal(5, entriesCount);
            Assert.Equal(5, relationLeftIDs.Intersect(relTablesLeft.Select(x => x.ID)).Count());

            // Checking restricted actions
            Assert.Throws<Exception>(() =>
            {
                try
                {
                    var relationLeftID = this._coreRepo.GetRelation(obj2.DXUnitDefinitionMainElement.Name, relTableFirstRight.ID, "RelTableLeftRelation");
                }
                catch (Exception exc)
                {
                    throw new Exception(exc.Message);
                }
            });
        }

        [Fact]
        public void CRUDRelations_UsingZeroOneToManyRelation_Success()
        {
            // Init
            var obj1Id = new Guid("fb372a16-bb98-4ed6-8491-fee1b6552e8d");
            var obj2Id = new Guid("bb387c55-0675-4ba8-9099-911ccabf9037");
            var objRelId = new Guid("9b65dff7-77ce-482d-b3d3-ad5a994a9dd5");

            var obj1 = ObjectFactory.GetItem(obj1Id, "RelTableLeft5");

            var obj2 = ObjectFactory.GetItem(obj2Id, "RelTableRight5");

            DXRelationDefinitionUnit objRelationInfo =
                DXRelationDefinitionUnitFactory.GetItem(objRelId,
                DXRelationTypeEnum.ZeroOneToMany,
                "RelTableLeftRelation",
                "RelTableRightRelation",
                obj1,
                obj2);

            int entriesCount = 5;
            var relTablesLeft = Enumerable.Range(0, entriesCount).Select(x => new RelTableLeft5() { ID = Guid.NewGuid() }).ToList();
            var relTablesRight = Enumerable.Range(0, entriesCount).Select(x => new RelTableRight5() { ID = Guid.NewGuid() }).ToList();

            var relTableFirstLeft = relTablesLeft.First();
            var relTableFirstRight = relTablesRight.First();

            base._finalizationAction = new Action(() =>
            {
                this._dataService.Delete(objRelationInfo);
                this._dataService.Delete(obj1);
                this._dataService.Delete(obj2);
            });

            this._dataService.Insert(obj1);
            this._dataService.Insert(obj2);

            // Action        
            this._dataService.Insert(objRelationInfo);

            // Checking
            var createdRelationEntry = this._genericRepo.GetItem<DXRelationDefinitionUnit>(objRelationInfo.ID);
            var createdInvertedRelationEntry = createdRelationEntry.CreateInvertedRelationObject();

            this.CheckDXRelationDefinitionUnit(objRelationInfo, createdRelationEntry);
            Assert.Null(createdRelationEntry.DXRelationDefinitionMainElement.RelationTable);

            this.CheckInvertedRelationEntry(createdRelationEntry, createdInvertedRelationEntry);

            // Init
            foreach (var item in relTablesLeft)
            {
                this._genericRepo.Insert(item);
            }

            foreach (var item in relTablesRight)
            {
                this._genericRepo.Insert(item);
            }

            // Checking relations between items
            var relTablesRightCreated = this._genericRepo.GetItems<RelTableRight5>(relTablesRight.Select(x => x.ID));

            Assert.Equal(entriesCount, relTablesRightCreated.Count());
            Assert.Equal(entriesCount, relTablesRightCreated.Select(x => x.ID).Intersect(relTablesRight.Select(x => x.ID)).Count());

            foreach (var item in relTablesRightCreated)
            {
                Assert.Null(item.RelTableLeftRelation);
            }

            this.CheckingMethodsForRelations(objRelationInfo, obj1, obj2, relTableFirstLeft, relTableFirstRight);

            // Additional checking
            foreach (var item in relTablesRight)
            {
                this._coreRepo.AddRelation(obj2.DXUnitDefinitionMainElement.Name, item.ID, "RelTableLeftRelation", obj1.DXUnitDefinitionMainElement.Name, relTableFirstLeft.ID);
            }

            foreach (var item in relTablesRight)
            {
                var relationLeftID = this._coreRepo.GetRelation(obj2.DXUnitDefinitionMainElement.Name, item.ID, "RelTableLeftRelation");
                var relationLeftIDs = this._coreRepo.GetRelations(obj2.DXUnitDefinitionMainElement.Name, item.ID, "RelTableLeftRelation");

                Assert.Equal(relTableFirstLeft.ID, relationLeftID);
                Assert.Single(relationLeftIDs);
                Assert.Equal(relTableFirstLeft.ID, relationLeftIDs.Single());
            }

            relTablesRightCreated = this._genericRepo.GetItems<RelTableRight5>(relTablesRight.Select(x => x.ID));

            foreach (var item in relTablesRightCreated)
            {
                Assert.Equal(relTableFirstLeft.ID, item.RelTableLeftRelation);
            }

            var relationRightIDs = this._coreRepo.GetRelations(obj1.DXUnitDefinitionMainElement.Name, relTableFirstLeft.ID, "RelTableRightRelation");

            Assert.Equal(5, entriesCount);
            Assert.Equal(5, relationRightIDs.Intersect(relTablesRight.Select(x => x.ID)).Count());

            // Checking restricted actions
            Assert.Throws<Exception>(() =>
            {
                try
                {
                    var relationRightID = this._coreRepo.GetRelation(obj1.DXUnitDefinitionMainElement.Name, relTableFirstLeft.ID, "RelTableLeftRelation");
                }
                catch (Exception exc)
                {
                    throw new Exception(exc.Message);
                }
            });
        }

        [Fact]
        public void CRUDRelations_UsingZeroOneToZeroOneRelation_Success()
        {
            // Init
            var obj1Id = new Guid("af737d0c-ed90-4017-bcec-7a09747ba1ba");
            var obj2Id = new Guid("c5bb4582-0707-4866-a137-459b4e4c4680");
            var objRelId = new Guid("c8785e82-54ef-4bd9-8d65-f0427b23df61");

            var obj1 = ObjectFactory.GetItem(obj1Id, "RelTableLeft8");

            var obj2 = ObjectFactory.GetItem(obj2Id, "RelTableRight8");

            DXRelationDefinitionUnit objRelationInfo =
                DXRelationDefinitionUnitFactory.GetItem(objRelId,
                DXRelationTypeEnum.ZeroOneToZeroOne,
                "RelTableLeftRelation",
                "RelTableRightRelation",
                obj1,
                obj2);

            int entriesCount = 5;
            var relTablesLeft = Enumerable.Range(0, entriesCount).Select(x => new RelTableLeft8() { ID = Guid.NewGuid() }).ToList();
            var relTablesRight = Enumerable.Range(0, entriesCount).Select(x => new RelTableRight8() { ID = Guid.NewGuid() }).ToList();

            var relTableFirstLeft = relTablesLeft.First();
            var relTableFirstRight = relTablesRight.First();

            base._finalizationAction = new Action(() =>
            {
                this._dataService.Delete(objRelationInfo);
                this._dataService.Delete(obj1);
                this._dataService.Delete(obj2);
            });

            this._dataService.Insert(obj1);
            this._dataService.Insert(obj2);

            // Action        
            this._dataService.Insert(objRelationInfo);

            // Checking
            var createdRelationEntry = this._genericRepo.GetItem<DXRelationDefinitionUnit>(objRelationInfo.ID);
            var createdInvertedRelationEntry = createdRelationEntry.CreateInvertedRelationObject();

            this.CheckDXRelationDefinitionUnit(objRelationInfo, createdRelationEntry);
            Assert.Equal("RelTableRight8", createdRelationEntry.DXRelationDefinitionMainElement.RelationTable);

            this.CheckInvertedRelationEntry(createdRelationEntry, createdInvertedRelationEntry);

            // Init
            foreach (var item in relTablesLeft)
            {
                this._genericRepo.Insert(item);
            }

            foreach (var item in relTablesRight)
            {
                this._genericRepo.Insert(item);
            }

            // Checking relations between items
            var relTablesRightCreated = this._genericRepo.GetItems<RelTableRight8>(relTablesRight.Select(x => x.ID));

            Assert.Equal(entriesCount, relTablesRightCreated.Count());
            Assert.Equal(entriesCount, relTablesRightCreated.Select(x => x.ID).Intersect(relTablesRight.Select(x => x.ID)).Count());

            foreach (var item in relTablesRightCreated)
            {
                Assert.Null(item.RelTableLeftRelation);
            }

            this.CheckingMethodsForRelations(objRelationInfo, obj1, obj2, relTableFirstLeft, relTableFirstRight);

            // Additional checking
            RelTableLeft8 lastItemLeft = null;

            foreach (var item in relTablesLeft)
            {
                this._coreRepo.AddRelation(obj1.DXUnitDefinitionMainElement.Name, item.ID, "RelTableRightRelation", obj2.DXUnitDefinitionMainElement.Name, relTableFirstRight.ID);

                var relationRightID = this._coreRepo.GetRelation(obj1.DXUnitDefinitionMainElement.Name, item.ID, "RelTableRightRelation");
                var relationRightIDs = this._coreRepo.GetRelations(obj1.DXUnitDefinitionMainElement.Name, item.ID, "RelTableRightRelation");

                Assert.Equal(relTableFirstRight.ID, relationRightID);
                Assert.Single(relationRightIDs);
                Assert.Equal(relTableFirstRight.ID, relationRightIDs.Single());

                lastItemLeft = item;
            }

            var relationLeftID = this._coreRepo.GetRelation(obj2.DXUnitDefinitionMainElement.Name, relTableFirstRight.ID, "RelTableLeftRelation");
            var relationLeftIDs = this._coreRepo.GetRelations(obj2.DXUnitDefinitionMainElement.Name, relTableFirstRight.ID, "RelTableLeftRelation");

            Assert.Equal(lastItemLeft.ID, relationLeftID);
            Assert.Single(relationLeftIDs);
            Assert.Equal(lastItemLeft.ID, relationLeftIDs.Single());
        }

        [ESQLObjectDefinition("RelTableLeft43a1")]
        private class RelTableLeft1 : ESQLObject
        {
        }

        [ESQLObjectDefinition("RelTableRight43a1")]
        private class RelTableRight1 : ESQLObject
        {
            [ESQLColumnDefinition("RelTableLeftRelation")]
            public Guid RelTableLeftRelation { get; set; }
        }

        [ESQLObjectDefinition("RelTableLeft2")]
        private class RelTableLeft2 : ESQLObject
        {
            [ESQLColumnDefinition("RelTableRightRelation")]
            public Guid RelTableRightRelation { get; set; }
        }

        [ESQLObjectDefinition("RelTableRight2")]
        private class RelTableRight2 : ESQLObject
        {
        }

        [ESQLObjectDefinition("RelTableLeft3")]
        private class RelTableLeft3 : ESQLObject
        {
        }

        [ESQLObjectDefinition("RelTableRight3")]
        private class RelTableRight3 : ESQLObject
        {
            [ESQLColumnDefinition("RelTableLeftRelation")]
            public Guid RelTableLeftRelation { get; set; }
        }

        [ESQLObjectDefinition("RelTableLeft4")]
        private class RelTableLeft4 : ESQLObject
        {
            [ESQLColumnDefinition("RelTableRightRelation")]
            public Guid RelTableRightRelation { get; set; }
        }

        [ESQLObjectDefinition("RelTableRight4")]
        private class RelTableRight4 : ESQLObject
        {
        }

        [ESQLObjectDefinition("RelTableLeft5")]
        private class RelTableLeft5 : ESQLObject
        {
        }

        [ESQLObjectDefinition("RelTableRight5")]
        private class RelTableRight5 : ESQLObject
        {
            [ESQLColumnDefinition("RelTableLeftRelation")]
            public Guid? RelTableLeftRelation { get; set; }
        }

        [ESQLObjectDefinition("RelTableLeft6")]
        private class RelTableLeft6 : ESQLObject
        {
            [ESQLColumnDefinition("RelTableRightRelation")]
            public Guid? RelTableRightRelation { get; set; }
        }

        [ESQLObjectDefinition("RelTableRight6")]
        private class RelTableRight6 : ESQLObject
        {
        }

        [ESQLObjectDefinition("RelTableLeft7")]
        private class RelTableLeft7 : ESQLObject
        {
        }

        [ESQLObjectDefinition("RelTableRight7")]
        private class RelTableRight7 : ESQLObject
        {
        }

        [ESQLObjectDefinition("RelTableLeft8")]
        private class RelTableLeft8 : ESQLObject
        {
        }

        [ESQLObjectDefinition("RelTableRight8")]
        private class RelTableRight8 : ESQLObject
        {
            [ESQLColumnDefinition("RelTableLeftRelation")]
            public Guid? RelTableLeftRelation { get; set; }
        }

        private void CheckDXRelationDefinitionUnit(DXRelationDefinitionUnit objRelationInfo, DXRelationDefinitionUnit createdEntry)
        {
            Assert.Equal(objRelationInfo.ID, createdEntry.ID);
            Assert.Equal(objRelationInfo.DXRelationDefinitionMainElement.ID, createdEntry.DXRelationDefinitionMainElement.ID);
            Assert.Equal(objRelationInfo.DXRelationDefinitionMainElement.ObjectID, createdEntry.DXRelationDefinitionMainElement.ObjectID);
            Assert.Equal(objRelationInfo.DXRelationDefinitionMainElement.RelationType, createdEntry.DXRelationDefinitionMainElement.RelationType);
            Assert.Equal(objRelationInfo.DXRelationDefinitionMainElement.RelationNameLeft, createdEntry.DXRelationDefinitionMainElement.RelationNameLeft);
            Assert.Equal(objRelationInfo.DXRelationDefinitionMainElement.RelationNameRight, createdEntry.DXRelationDefinitionMainElement.RelationNameRight);
            Assert.Equal(objRelationInfo.DXRelationDefinitionMainElement.ObjectNameLeft, createdEntry.DXRelationDefinitionMainElement.ObjectNameLeft);
            Assert.Equal(objRelationInfo.DXRelationDefinitionMainElement.ObjectNameRight, createdEntry.DXRelationDefinitionMainElement.ObjectNameRight);
        }

        private void CheckInvertedRelationEntry(DXRelationDefinitionUnit createdRelationEntry, DXRelationDefinitionUnit createdInvertedRelationEntry)
        {
            Assert.Equal(createdRelationEntry.DXRelationDefinitionMainElement.ObjectNameLeft, createdInvertedRelationEntry.DXRelationDefinitionMainElement.ObjectNameRight);
            Assert.Equal(createdRelationEntry.DXRelationDefinitionMainElement.ObjectNameRight, createdInvertedRelationEntry.DXRelationDefinitionMainElement.ObjectNameLeft);
            Assert.Equal(createdRelationEntry.DXRelationDefinitionMainElement.RelationNameLeft, createdInvertedRelationEntry.DXRelationDefinitionMainElement.RelationNameRight);
            Assert.Equal(createdRelationEntry.DXRelationDefinitionMainElement.RelationNameRight, createdInvertedRelationEntry.DXRelationDefinitionMainElement.RelationNameLeft);
        }

        private void CheckingMethodsForRelations(
            DXRelationDefinitionUnit relationInfo,
            DPObjectDescObject objLeft,
            DPObjectDescObject objRight,
            ESQLObject relTableFirstLeft,
            ESQLObject relTableFirstRight)
        {
            #region Checking core repo to process N to M relation using one entry from left list and one entry from right list
            var actionsForAdding = new List<Action>() {
                new Action(() =>
                {
                    this._coreRepo.AddRelation(objLeft.DXUnitDefinitionMainElement.Name, relTableFirstLeft.ID, "RelTableRightRelation", objRight.DXUnitDefinitionMainElement.Name, relTableFirstRight.ID);
                }),
                new Action(() =>
                {
                    this._coreRepo.AddRelation(objRight.DXUnitDefinitionMainElement.Name, relTableFirstRight.ID, "RelTableLeftRelation", objLeft.DXUnitDefinitionMainElement.Name, relTableFirstLeft.ID);
                }),
            };
            var actionsForRemoving = new List<Action>() {
                new Action(() =>
                {
                    this._coreRepo.RemoveRelation(objLeft.DXUnitDefinitionMainElement.Name, relTableFirstLeft.ID, "RelTableRightRelation", objRight.DXUnitDefinitionMainElement.Name, relTableFirstRight.ID);
                }),
                new Action(() =>
                {
                    this._coreRepo.RemoveRelation(objRight.DXUnitDefinitionMainElement.Name, relTableFirstRight.ID, "RelTableLeftRelation", objLeft.DXUnitDefinitionMainElement.Name, relTableFirstLeft.ID);
                }),
            };

            foreach (var actionForAdding in actionsForAdding)
            {
                foreach (var actionForRemoving in actionsForRemoving)
                {
                    // Action
                    actionForAdding.Invoke();

                    // Checking
                    var relationIdRight = this._coreRepo.GetRelation(objLeft.DXUnitDefinitionMainElement.Name, relTableFirstLeft.ID, "RelTableRightRelation");
                    var relationIdsRight = this._coreRepo.GetRelations(objLeft.DXUnitDefinitionMainElement.Name, relTableFirstLeft.ID, "RelTableRightRelation");

                    Assert.Equal(relTableFirstRight.ID, relationIdRight);
                    Assert.Single(relationIdsRight);
                    Assert.Equal(relTableFirstRight.ID, relationIdsRight.Single());

                    var relationIdLeft = this._coreRepo.GetRelation(objRight.DXUnitDefinitionMainElement.Name, relTableFirstRight.ID, "RelTableLeftRelation");
                    var relationIdsLeft = this._coreRepo.GetRelations(objRight.DXUnitDefinitionMainElement.Name, relTableFirstRight.ID, "RelTableLeftRelation");

                    Assert.Equal(relTableFirstLeft.ID, relationIdLeft);
                    Assert.Single(relationIdsLeft);
                    Assert.Equal(relTableFirstLeft.ID, relationIdsLeft.Single());

                    if (relationInfo.DXRelationDefinitionMainElement.RelationType != DXRelationTypeEnum.ManyToOne
                        && relationInfo.DXRelationDefinitionMainElement.RelationType != DXRelationTypeEnum.OneToMany
                        && relationInfo.DXRelationDefinitionMainElement.RelationType != DXRelationTypeEnum.OneToZeroOne
                        && relationInfo.DXRelationDefinitionMainElement.RelationType != DXRelationTypeEnum.ZeroOneToOne)
                    {
                        // Action
                        actionForRemoving.Invoke();

                        relationIdRight = this._coreRepo.GetRelation(objLeft.DXUnitDefinitionMainElement.Name, relTableFirstLeft.ID, "RelTableRightRelation");
                        relationIdsRight = this._coreRepo.GetRelations(objLeft.DXUnitDefinitionMainElement.Name, relTableFirstLeft.ID, "RelTableRightRelation");
                        relationIdLeft = this._coreRepo.GetRelation(objRight.DXUnitDefinitionMainElement.Name, relTableFirstRight.ID, "RelTableLeftRelation");
                        relationIdsLeft = this._coreRepo.GetRelations(objRight.DXUnitDefinitionMainElement.Name, relTableFirstRight.ID, "RelTableLeftRelation");

                        Assert.Null(relationIdRight);
                        Assert.Null(relationIdLeft);
                        Assert.Empty(relationIdsRight);
                        Assert.Empty(relationIdsLeft);
                    }
                }
            }
            #endregion
        }
    }
}