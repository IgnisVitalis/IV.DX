namespace IV.DX.Persistence.IntTests.Test
{
    //public class InitTestDataStructure : IntTestController
    //{
    //    //[Fact]
    //    public void CreateUserStructure()
    //    {
    //        // Init
    //        var userObject = new DXObjectDefinitionUnit()
    //        {
    //            ID = new Guid("1faf325f-57bc-4ab2-bb3c-03a6ab5ae859"),
    //            DXUnitDefinitionMainElement = new DXUnitDefinitionMainElement()
    //            {
    //                ID = new Guid("b0f45798-4fc0-48e4-b791-a31e36d16e3b"),
    //                ObjectID = new Guid("1faf325f-57bc-4ab2-bb3c-03a6ab5ae859"),
    //                Name = "TUserUnit"
    //            }
    //        };

    //        var userGenBlock = new DXObjectDefinitionUnit()
    //        {
    //            ID = new Guid("515b9785-6bbc-40b6-8af6-2d862d15b60b"),
    //            DXUnitDefinitionMainElement = new DXUnitDefinitionMainElement()
    //            {
    //                ID = new Guid("bea190b6-4138-4775-9157-f3b15ac9d51e"),
    //                ObjectID = new Guid("515b9785-6bbc-40b6-8af6-2d862d15b60b"),
    //                Name = "TUserMainElement"
    //            },
    //            DXColumnDefinitionElement = new ESQLMultiItemsContainer<DXColumnDefinitionElement>()
    //            {
    //                Announced = new List<DXColumnDefinitionElement>()
    //                {
    //                    new DXColumnDefinitionElement()
    //                    {
    //                        ID = new Guid("c9cd8a6a-1987-4320-933d-128420e55fd5"),
    //                        ObjectID = new Guid("515b9785-6bbc-40b6-8af6-2d862d15b60b"),
    //                        Name = "Name",
    //                        ColumnType = DXColumnTypeEnum.String,
    //                        Length = 50,
    //                        AllowNull = false
    //                    },
    //                    new DXColumnDefinitionElement()
    //                    {
    //                        ID = new Guid("499867bb-8ce6-4245-862d-b7486e58238a"),
    //                        ObjectID = new Guid("515b9785-6bbc-40b6-8af6-2d862d15b60b"),
    //                        Name = "Birth",
    //                        ColumnType = DXColumnTypeEnum.DateTime,
    //                        AllowNull = false
    //                    }
    //                }
    //            }
    //        };

    //        var blockInObjectInfo = new DPBlockInObjectDescObject()
    //        {
    //            ID = new Guid("7f052b48-7008-4418-84fe-e51d42e2170d"),
    //            DPBlock = userGenBlock.ID,
    //            DPObject = userObject.ID,
    //            DPBlockInObjectDescGenBlock = new DPBlockInObjectDescGenBlock()
    //            {
    //                ID = new Guid("55a9d478-e4ef-4bb2-8743-290216b22979"),
    //                ObjectID = new Guid("7f052b48-7008-4418-84fe-e51d42e2170d"),
    //                DXElementInUnitTypeEnum = DXElementInUnitTypeEnum.SingleMandatory,
    //            }
    //        };

    //        // Action
    //        this._dataService.Insert(userObject);
    //        this._dataService.Insert(userGenBlock);
    //        this._dataService.Insert(blockInObjectInfo);
    //    }

    //    //[Fact]
    //    public void UpdateUserStructure()
    //    {
    //        var userGenBlock = new DXObjectDefinitionUnit()
    //        {
    //            ID = new Guid("515b9785-6bbc-40b6-8af6-2d862d15b60b"),
    //            DXUnitDefinitionMainElement = new DXUnitDefinitionMainElement()
    //            {
    //                ID = new Guid("bea190b6-4138-4775-9157-f3b15ac9d51e"),
    //                ObjectID = new Guid("515b9785-6bbc-40b6-8af6-2d862d15b60b"),
    //                Name = "TUserMainElement"
    //            },
    //            DXColumnDefinitionElement = new ESQLMultiItemsContainer<DXColumnDefinitionElement>()
    //            {
    //                Announced = new List<DXColumnDefinitionElement>()
    //                {
    //                    new DXColumnDefinitionElement()
    //                    {
    //                        ID = new Guid("c9cd8a6a-1987-4320-933d-128420e55fd5"),
    //                        ObjectID = new Guid("515b9785-6bbc-40b6-8af6-2d862d15b60b"),
    //                        Name = "Name",
    //                        ColumnType = DXColumnTypeEnum.String,
    //                        Length = 50,
    //                        AllowNull = false
    //                    },
    //                    new DXColumnDefinitionElement()
    //                    {
    //                        ID = new Guid("499867bb-8ce6-4245-862d-b7486e58238a"),
    //                        ObjectID = new Guid("515b9785-6bbc-40b6-8af6-2d862d15b60b"),
    //                        Name = "Birth",
    //                        ColumnType = DXColumnTypeEnum.DateTime,
    //                        AllowNull = false
    //                    },
    //                    new DXColumnDefinitionElement()
    //                    {
    //                        ID = new Guid("66d24eb6-b30d-4a9a-a31c-c2b7c8e8c69e"),
    //                        ObjectID = new Guid("515b9785-6bbc-40b6-8af6-2d862d15b60b"),
    //                        Name = "Surname",
    //                        ColumnType = DXColumnTypeEnum.String,
    //                        Length = 50,
    //                        AllowNull = false
    //                    },
    //                }
    //            }
    //        };

    //        this._dataService.Update(userGenBlock);
    //    }

    //    //[Fact]
    //    public void CreatePassportStructure()
    //    {
    //        // Init
    //        var obj = new DXObjectDefinitionUnit()
    //        {
    //            ID = new Guid("8e2c5365-85c0-431d-996e-fbccfe3f856a"),
    //            DXUnitDefinitionMainElement = new DXUnitDefinitionMainElement()
    //            {
    //                ID = new Guid("29f74c7a-4d82-4140-a88e-1a25a68e67ef"),
    //                ObjectID = new Guid("8e2c5365-85c0-431d-996e-fbccfe3f856a"),
    //                Name = "TPassportUnit"
    //            }
    //        };

    //        var genBlock = new DXObjectDefinitionUnit()
    //        {
    //            ID = new Guid("714ee242-8821-4a4f-a28e-c623004d49a4"),
    //            DXUnitDefinitionMainElement = new DXUnitDefinitionMainElement()
    //            {
    //                ID = new Guid("2218f1da-5c87-424f-843d-964fa095f480"),
    //                ObjectID = new Guid("714ee242-8821-4a4f-a28e-c623004d49a4"),
    //                Name = "TPassportMainElement"
    //            },
    //            DXColumnDefinitionElement = new ESQLMultiItemsContainer<DXColumnDefinitionElement>()
    //            {
    //                Announced = new List<DXColumnDefinitionElement>()
    //                {
    //                    new DXColumnDefinitionElement()
    //                    {
    //                        ID = new Guid("d6c1126f-1a54-439e-8834-c1b4ba1dd727"),
    //                        ObjectID = new Guid("714ee242-8821-4a4f-a28e-c623004d49a4"),
    //                        Name = "SerialNumber",
    //                        ColumnType = DXColumnTypeEnum.String,
    //                        Length = 50,
    //                        AllowNull = false
    //                    }
    //                }
    //            }
    //        };

    //        var blockInObjectInfo = new DPBlockInObjectDescObject()
    //        {
    //            ID = new Guid("72f5d23f-2fd5-44ac-886a-3da7dd3f70ea"),
    //            DPBlock = genBlock.ID,
    //            DPObject = obj.ID,
    //            DPBlockInObjectDescGenBlock = new DPBlockInObjectDescGenBlock()
    //            {
    //                ID = new Guid("fd309653-967e-40e1-862f-92b906689d70"),
    //                ObjectID = new Guid("72f5d23f-2fd5-44ac-886a-3da7dd3f70ea"),
    //                DXElementInUnitTypeEnum = DXElementInUnitTypeEnum.SingleMandatory,
    //            }
    //        };

    //        // Action
    //        this._dataService.Insert(obj);
    //        this._dataService.Insert(genBlock);
    //        this._dataService.Insert(blockInObjectInfo);
    //    }

    //    //[Fact]
    //    public void CreateUserPassportRelation()
    //    {
    //        // Init
    //        var objRelId = new Guid("f15f71da-ab49-4937-911a-58170f32da30");
    //        var obj1 = this._genericRepo.GetItem<DXObjectDefinitionUnit>(new Guid("1faf325f-57bc-4ab2-bb3c-03a6ab5ae859"));
    //        var obj2 = this._genericRepo.GetItem<DXObjectDefinitionUnit>(new Guid("8e2c5365-85c0-431d-996e-fbccfe3f856a"));

    //        var relationInfo =
    //                DXRelationDefinitionUnitFactory.GetItem(objRelId,
    //                DXRelationTypeEnum.OneToZeroOne,
    //                "User",
    //                "Passport",
    //                obj1,
    //                obj2);

    //        // Action
    //        //this._dataService.Delete(this._genericRepo.GetItem<DXRelationDefinitionUnit>(new Guid("f15f71da-ab49-4937-911a-58170f32da30")));
    //        this._dataService.Insert(relationInfo);
    //    }

    //    //[Fact]
    //    public void UpdateDeviceStructure()
    //    {
    //        // Init   
    //        var genBlock = new DXObjectDefinitionUnit()
    //        {
    //            ID = new Guid("37099cee-e2cd-4d86-bece-8e7a11a96da2"),
    //            DXUnitDefinitionMainElement = new DXUnitDefinitionMainElement()
    //            {
    //                ID = new Guid("57a80968-3195-427b-a49d-8ba69d472f2c"),
    //                ObjectID = new Guid("37099cee-e2cd-4d86-bece-8e7a11a96da2"),
    //                Name = "TDeviceMainElement"
    //            },
    //            DXColumnDefinitionElement = new ESQLMultiItemsContainer<DXColumnDefinitionElement>()
    //            {
    //                Announced = new List<DXColumnDefinitionElement>()
    //                {
    //                    new DXColumnDefinitionElement()
    //                    {
    //                        ID = new Guid("58cc9980-87d0-469b-8b66-33e90962f3ab"),
    //                        ObjectID = new Guid("37099cee-e2cd-4d86-bece-8e7a11a96da2"),
    //                        Name = "Model",
    //                        ColumnType = DXColumnTypeEnum.String,
    //                        Length = 50,
    //                        AllowNull = false
    //                    },
    //                    new DXColumnDefinitionElement()
    //                    {
    //                        ID = new Guid("9de64e8a-efaa-4759-8e5d-8ec4ff198dd3"),
    //                        ObjectID = new Guid("37099cee-e2cd-4d86-bece-8e7a11a96da2"),
    //                        Name = "UUID",
    //                        ColumnType = DXColumnTypeEnum.GUID,
    //                        AllowNull = false
    //                    }
    //                }
    //            }
    //        };

    //        // Action           
    //        this._dataService.Update(genBlock);
    //    }

    //    //[Fact]
    //    public void CreateUserDeviceRelation()
    //    {
    //        // Init
    //        var objRelId = new Guid("3e9be76e-a2d2-4ff4-9c93-8a4df4846066");
    //        var obj1 = this._genericRepo.GetItem<DXObjectDefinitionUnit>(new Guid("1faf325f-57bc-4ab2-bb3c-03a6ab5ae859"));
    //        var obj2 = this._genericRepo.GetItem<DXObjectDefinitionUnit>(new Guid("356aaa53-fc71-41dd-90a0-53975d938cf9"));

    //        var relationInfo =
    //                DXRelationDefinitionUnitFactory.GetItem(objRelId,
    //                DXRelationTypeEnum.OneToMany,
    //                "User",
    //                "Devices",
    //                obj1,
    //                obj2);

    //        // Action
    //        //this._dataService.Delete(this._genericRepo.GetItem<DXRelationDefinitionUnit>(new Guid("3e9be76e-a2d2-4ff4-9c93-8a4df4846066")));
    //        this._dataService.Insert(relationInfo);
    //    }

    //    //[Fact]
    //    public void CreatePositionStructure()
    //    {
    //        // Init
    //        var obj = new DXObjectDefinitionUnit()
    //        {
    //            ID = new Guid("394566d6-93e4-446a-800d-2209898475ac"),
    //            DXUnitDefinitionMainElement = new DXUnitDefinitionMainElement()
    //            {
    //                ID = new Guid("33b7bf0d-5761-49eb-9d0c-1c6b82fb9304"),
    //                ObjectID = new Guid("394566d6-93e4-446a-800d-2209898475ac"),
    //                Name = "TPositionUnit"
    //            }
    //        };

    //        var genBlock = new DXObjectDefinitionUnit()
    //        {
    //            ID = new Guid("408a680c-4012-4dc8-ad8f-2676f699734f"),
    //            DXUnitDefinitionMainElement = new DXUnitDefinitionMainElement()
    //            {
    //                ID = new Guid("0a0245c5-65ba-4be1-8747-da4a72974053"),
    //                ObjectID = new Guid("408a680c-4012-4dc8-ad8f-2676f699734f"),
    //                Name = "TPositionMainElement"
    //            },
    //            DXColumnDefinitionElement = new ESQLMultiItemsContainer<DXColumnDefinitionElement>()
    //            {
    //                Announced = new List<DXColumnDefinitionElement>()
    //                {
    //                    new DXColumnDefinitionElement()
    //                    {
    //                        ID = new Guid("6e021c8b-8b81-493a-bc38-bd84a7cd40de"),
    //                        ObjectID = new Guid("408a680c-4012-4dc8-ad8f-2676f699734f"),
    //                        Name = "Name",
    //                        ColumnType = DXColumnTypeEnum.String,
    //                        Length = 50,
    //                        AllowNull = false
    //                    }
    //                }
    //            }
    //        };

    //        var blockInObjectInfo = new DPBlockInObjectDescObject()
    //        {
    //            ID = new Guid("fc4ec5ba-6371-4824-8123-603b62df32f4"),
    //            DPObject = obj.ID,
    //            DPBlock = genBlock.ID,
    //            DPBlockInObjectDescGenBlock = new DPBlockInObjectDescGenBlock()
    //            {
    //                ID = new Guid("d598414d-5c48-40c5-ba7a-b56a871e62b5"),
    //                ObjectID = new Guid("fc4ec5ba-6371-4824-8123-603b62df32f4"),
    //                DXElementInUnitTypeEnum = DXElementInUnitTypeEnum.SingleMandatory,
    //            }
    //        };

    //        // Action
    //        this._dataService.Insert(obj);
    //        this._dataService.Insert(genBlock);
    //        this._dataService.Insert(blockInObjectInfo);
    //    }

    //    //Fact]
    //    public void CreateUserPositionRelation()
    //    {
    //        // Init
    //        var objRelId = new Guid("8ab5ba94-ca7c-47ef-99a8-dfb9c020af92");
    //        var obj1 = this._genericRepo.GetItem<DXObjectDefinitionUnit>(new Guid("1faf325f-57bc-4ab2-bb3c-03a6ab5ae859"));
    //        var obj2 = this._genericRepo.GetItem<DXObjectDefinitionUnit>(new Guid("394566d6-93e4-446a-800d-2209898475ac"));

    //        var relationInfo =
    //                DXRelationDefinitionUnitFactory.GetItem(objRelId,
    //                DXRelationTypeEnum.ZeroOneToZeroOne,
    //                "User",
    //                "Position",
    //                obj1,
    //                obj2);

    //        // Action
    //        //this._dataService.Delete(this._genericRepo.GetItem<DXRelationDefinitionUnit>(new Guid("3e9be76e-a2d2-4ff4-9c93-8a4df4846066")));
    //        this._dataService.Insert(relationInfo);
    //    }

    //    //[Fact]
    //    public void CreateDocumentStructure()
    //    {
    //        // Init
    //        var obj = new DXObjectDefinitionUnit()
    //        {
    //            ID = new Guid("3dae1265-e917-4b91-b4c3-f3f835281630"),
    //            DXUnitDefinitionMainElement = new DXUnitDefinitionMainElement()
    //            {
    //                ID = new Guid("a3a03877-5021-4bb9-9020-ef017b69ce48"),
    //                ObjectID = new Guid("3dae1265-e917-4b91-b4c3-f3f835281630"),
    //                Name = "TDocumentUnit"
    //            }
    //        };

    //        var genBlock = new DXObjectDefinitionUnit()
    //        {
    //            ID = new Guid("29a4d2f3-0f2a-4a60-a12e-8c4dd1af8476"),
    //            DXUnitDefinitionMainElement = new DXUnitDefinitionMainElement()
    //            {
    //                ID = new Guid("6244f906-587d-4a2c-b3a1-54b90a110e9a"),
    //                ObjectID = new Guid("29a4d2f3-0f2a-4a60-a12e-8c4dd1af8476"),
    //                Name = "TDocumentMainElement"
    //            },
    //            DXColumnDefinitionElement = new ESQLMultiItemsContainer<DXColumnDefinitionElement>()
    //            {
    //                Announced = new List<DXColumnDefinitionElement>()
    //                {
    //                    new DXColumnDefinitionElement()
    //                    {
    //                        ID = new Guid("dc864e8b-c84f-48a1-8e22-1ecedd742c5f"),
    //                        ObjectID = new Guid("29a4d2f3-0f2a-4a60-a12e-8c4dd1af8476"),
    //                        Name = "Name",
    //                        ColumnType = DXColumnTypeEnum.String,
    //                        Length = 50,
    //                        AllowNull = false
    //                    }
    //                }
    //            }
    //        };

    //        var blockInObjectInfo = new DPBlockInObjectDescObject()
    //        {
    //            ID = new Guid("a132f7ef-5bf2-4ebf-bc51-c95f6eddd78c"),
    //            DPObject = obj.ID,
    //            DPBlock = genBlock.ID,
    //            DPBlockInObjectDescGenBlock = new DPBlockInObjectDescGenBlock()
    //            {
    //                ID = new Guid("3109ea24-f209-4aac-9414-e7f3493aa41a"),
    //                ObjectID = new Guid("a132f7ef-5bf2-4ebf-bc51-c95f6eddd78c"),
    //                DXElementInUnitTypeEnum = DXElementInUnitTypeEnum.SingleMandatory,
    //            }
    //        };

    //        // Action
    //        this._dataService.Insert(obj);
    //        this._dataService.Insert(genBlock);
    //        this._dataService.Insert(blockInObjectInfo);
    //    }

    //    //[Fact]
    //    public void CreateUserDocumentRelation()
    //    {
    //        // Init
    //        var objRelId = new Guid("e799893d-0943-4902-86aa-9a21747cf764");
    //        var obj1 = this._genericRepo.GetItem<DXObjectDefinitionUnit>(new Guid("1faf325f-57bc-4ab2-bb3c-03a6ab5ae859"));
    //        var obj2 = this._genericRepo.GetItem<DXObjectDefinitionUnit>(new Guid("3dae1265-e917-4b91-b4c3-f3f835281630"));

    //        var relationInfo =
    //                DXRelationDefinitionUnitFactory.GetItem(objRelId,
    //                DXRelationTypeEnum.ZeroOneToMany,
    //                "User",
    //                "Documents",
    //                obj1,
    //                obj2);

    //        // Action
    //        //this._dataService.Delete(this._genericRepo.GetItem<DXRelationDefinitionUnit>(new Guid("3e9be76e-a2d2-4ff4-9c93-8a4df4846066")));
    //        this._dataService.Insert(relationInfo);
    //    }

    //    //[Fact]
    //    public void CreateBookStructure()
    //    {
    //        // Init
    //        var obj = new DXObjectDefinitionUnit()
    //        {
    //            ID = new Guid("6555d7f8-27a6-495d-91e3-df0a49354032"),
    //            DXUnitDefinitionMainElement = new DXUnitDefinitionMainElement()
    //            {
    //                ID = new Guid("b2561ad0-c5ef-40dd-b460-c7c1330b3e54"),
    //                ObjectID = new Guid("6555d7f8-27a6-495d-91e3-df0a49354032"),
    //                Name = "TBookUnit"
    //            }
    //        };

    //        var genBlock1 = new DXObjectDefinitionUnit()
    //        {
    //            ID = new Guid("0ccee9e3-67cb-4692-940c-41929f9df7b0"),
    //            DXUnitDefinitionMainElement = new DXUnitDefinitionMainElement()
    //            {
    //                ID = new Guid("6e6839b6-8dc6-4e15-86b4-0767f3042f11"),
    //                ObjectID = new Guid("0ccee9e3-67cb-4692-940c-41929f9df7b0"),
    //                Name = "TBookMainElement"
    //            },
    //            DXColumnDefinitionElement = new ESQLMultiItemsContainer<DXColumnDefinitionElement>()
    //            {
    //                Announced = new List<DXColumnDefinitionElement>()
    //                {
    //                    new DXColumnDefinitionElement()
    //                    {
    //                        ID = new Guid("665b5c55-eb32-488d-8305-1c575344863e"),
    //                        ObjectID = new Guid("0ccee9e3-67cb-4692-940c-41929f9df7b0"),
    //                        Name = "Name",
    //                        ColumnType = DXColumnTypeEnum.String,
    //                        Length = 50,
    //                        AllowNull = false
    //                    }
    //                }
    //            }
    //        };

    //        var genBlock2 = new DXObjectDefinitionUnit()
    //        {
    //            ID = new Guid("28644277-705c-4666-8b7b-e33066e2ea27"),
    //            DXUnitDefinitionMainElement = new DXUnitDefinitionMainElement()
    //            {
    //                ID = new Guid("06560692-98ca-41e6-9763-331a2aed5a1f"),
    //                ObjectID = new Guid("28644277-705c-4666-8b7b-e33066e2ea27"),
    //                Name = "TBookChapterElement"
    //            },
    //            DXColumnDefinitionElement = new ESQLMultiItemsContainer<DXColumnDefinitionElement>()
    //            {
    //                Announced = new List<DXColumnDefinitionElement>()
    //                {
    //                    new DXColumnDefinitionElement()
    //                    {
    //                        ID = new Guid("9c48b070-0332-49ac-982e-ced1bab74368"),
    //                        ObjectID = new Guid("28644277-705c-4666-8b7b-e33066e2ea27"),
    //                        Name = "Text",
    //                        ColumnType = DXColumnTypeEnum.Text,
    //                        AllowNull = false
    //                    },
    //                    new DXColumnDefinitionElement()
    //                    {
    //                        ID = new Guid("b0148a60-48c8-4c26-9ab8-fae46037577b"),
    //                        ObjectID = new Guid("28644277-705c-4666-8b7b-e33066e2ea27"),
    //                        Name = "Number",
    //                        ColumnType = DXColumnTypeEnum.Int,
    //                        AllowNull = false
    //                    }
    //                }
    //            }
    //        };

    //        var blockInObjectInfo1 = new DPBlockInObjectDescObject()
    //        {
    //            ID = new Guid("db3cdce4-a71f-4129-8d1b-a1b8662ad1dd"),
    //            DPObject = obj.ID,
    //            DPBlock = genBlock1.ID,
    //            DPBlockInObjectDescGenBlock = new DPBlockInObjectDescGenBlock()
    //            {
    //                ID = new Guid("07fb6348-a3f8-4b6c-924d-f6533316156e"),
    //                ObjectID = new Guid("db3cdce4-a71f-4129-8d1b-a1b8662ad1dd"),
    //                DXElementInUnitTypeEnum = DXElementInUnitTypeEnum.SingleMandatory,
    //            }
    //        };

    //        var blockInObjectInfo2 = new DPBlockInObjectDescObject()
    //        {
    //            ID = new Guid("f9bf6850-49e3-4515-9299-4a9f07674b22"),
    //            DPObject = obj.ID,
    //            DPBlock = genBlock2.ID,
    //            DPBlockInObjectDescGenBlock = new DPBlockInObjectDescGenBlock()
    //            {
    //                ID = new Guid("963b1a70-bff1-49c4-8360-f7c34c02b2cb"),
    //                ObjectID = new Guid("f9bf6850-49e3-4515-9299-4a9f07674b22"),
    //                DXElementInUnitTypeEnum = DXElementInUnitTypeEnum.MultiOptional,
    //            }
    //        };

    //        // Action
    //        this._dataService.Insert(obj);
    //        this._dataService.Insert(genBlock1);
    //        this._dataService.Insert(blockInObjectInfo1);
    //        this._dataService.Insert(genBlock2);
    //        this._dataService.Insert(blockInObjectInfo2);
    //    }

    //    //[Fact]
    //    public void CreateUserBookRelation()
    //    {
    //        // Init
    //        var objRelId = new Guid("ef5e9942-c1bb-4637-97a1-b95b2f843a50");
    //        var obj1 = this._genericRepo.GetItem<DXObjectDefinitionUnit>(new Guid("1faf325f-57bc-4ab2-bb3c-03a6ab5ae859"));
    //        var obj2 = this._genericRepo.GetItem<DXObjectDefinitionUnit>(new Guid("6555d7f8-27a6-495d-91e3-df0a49354032"));

    //        var relationInfo =
    //                DXRelationDefinitionUnitFactory.GetItem(objRelId,
    //                DXRelationTypeEnum.ManyToMany,
    //                "Users",
    //                "Books",
    //                obj1,
    //                obj2);

    //        // Action
    //        //this._dataService.Delete(this._genericRepo.GetItem<DXRelationDefinitionUnit>(new Guid("3e9be76e-a2d2-4ff4-9c93-8a4df4846066")));
    //        this._dataService.Insert(relationInfo);
    //    }
    //}
}