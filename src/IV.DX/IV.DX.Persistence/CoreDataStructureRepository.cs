using IV.DataProvider.Persistence.Contracts.Models;
using IV.DX.Contracts.Common.Enums;
using IV.DX.Contracts.Common.Models;

namespace IV.DX.Persistence
{
    internal static class CoreDataStructureRepository
    {
        public static IEnumerable<DPBlockDescObject> CoreBlockInfos { get; set; }
        public static IEnumerable<DPEnumDescObject> CoreEnumInfos { get; set; }
        public static IEnumerable<DPEntityDescObject> CoreEntityInfos { get; set; }
        public static IEnumerable<DPRelationObject> CoreRelationInfos { get; set; }

        static CoreDataStructureRepository()
        {
            InitCoreEnumInfos();
            InitCoreBlockInfos();
            InitCoreEntityInfos();
        }

        private static void InitCoreEnumInfos()
        {
            #region DPBlockInObjectTypeEnum
            var dpBlockInObjectTypeEnum = new DPEnumDescObject()
            {
                ID = new Guid("5e8630a5-e51a-4717-b63e-92a176e2aa8e"),
                DPObjectDescGenBlock = new DPObjectDescGenBlock()
                {
                    ID = new Guid("59b115c5-cc8b-4c88-b1e0-d94af8745e99"),
                    Name = "DPBlockInObjectTypeEnum",
                    Kind = DPObjectKindEnum.Core,
                },
                DPColumnDescBlock = new ESQLMultiItemsContainer<DPColumnDescBlock>()
                {
                    Mode = ModeForMultiItems.Full,
                    Announced = new List<DPColumnDescBlock>()
                    {
                        new DPColumnDescBlock()
                        {
                            ID = new Guid("72d914ae-c902-45dd-b405-fb12d5021597"),
                            ColumnType = DPColumnTypeEnum.String,
                            Name = "Value",
                            Length = 50
                        },
                        new DPColumnDescBlock()
                        {
                            ID = new Guid("d214fd3a-99a2-4446-ba90-dd1e863e56e1"),
                            ColumnType = DPColumnTypeEnum.Int,
                            Name = "Key"
                        }
                    }
                },
                DPColumnsUniqueBlock = new ESQLMultiItemsContainer<DPColumnsUniqueBlock>()
                {
                    Mode = ModeForMultiItems.Full,
                    Announced = new List<DPColumnsUniqueBlock>()
                    {
                        new DPColumnsUniqueBlock()
                        {
                            ID = new Guid("44892f0b-5f87-4373-b3c2-a7eac4a41ffd"),
                            Columns = "Key"
                        }
                    }
                }
            };
            #endregion

            #region DPColumnTypeEnum
            var dpColumnTypeEnum = new DPEnumDescObject()
            {
                ID = new Guid("971d538a-1489-483a-bc84-86596ed0c51a"),
                DPObjectDescGenBlock = new DPObjectDescGenBlock()
                {
                    ID = new Guid("261cfd33-65cb-4f36-8712-be15e6621cc0"),
                    Name = "DPColumnTypeEnum",
                    Kind = DPObjectKindEnum.Core
                },
                DPColumnDescBlock = new ESQLMultiItemsContainer<DPColumnDescBlock>()
                {
                    Mode = ModeForMultiItems.Full,
                    Announced = new List<DPColumnDescBlock>()
                    {
                        new DPColumnDescBlock()
                        {
                            ID = new Guid("940d5067-9271-4dc2-9ae0-b1b83c519e11"),
                            ColumnType = DPColumnTypeEnum.String,
                            Name = "Value",
                            Length = 50
                        },
                        new DPColumnDescBlock()
                        {
                            ID = new Guid("884bb1f2-9eaf-4fa1-8bc1-9057edf5b5d2"),
                            ColumnType = DPColumnTypeEnum.Int,
                            Name = "Key"
                        }
                    }
                },
                DPColumnsUniqueBlock = new ESQLMultiItemsContainer<DPColumnsUniqueBlock>()
                {
                    Mode = ModeForMultiItems.Full,
                    Announced = new List<DPColumnsUniqueBlock>()
                    {
                        new DPColumnsUniqueBlock()
                        {
                            ID = new Guid("ff9aab92-03c6-4c38-8147-08c77e694fe9"),
                            Columns = "Key"
                        }
                    }
                }
            };

            #endregion

            #region DPObjectKindEnum
            var dpObjectKindEnum = new DPEnumDescObject()
            {
                ID = new Guid("3c9d2fa6-99e3-472b-b493-3e4790597f98"),
                DPObjectDescGenBlock = new DPObjectDescGenBlock()
                {
                    ID = new Guid("179f9be7-dc54-4ac7-a9c5-ea50c7524752"),
                    Name = "DPObjectKindEnum",
                    Kind = DPObjectKindEnum.Core
                },
                DPColumnDescBlock = new ESQLMultiItemsContainer<DPColumnDescBlock>()
                {
                    Mode = ModeForMultiItems.Full,
                    Announced = new List<DPColumnDescBlock>()
                    {
                        new DPColumnDescBlock()
                        {
                            ID = new Guid("6212d559-2cf3-4341-a517-89f3a57abe78"),
                            ColumnType = DPColumnTypeEnum.String,
                            Name = "Value",
                            Length = 50
                        },
                        new DPColumnDescBlock()
                        {
                            ID = new Guid("15d97f21-fd2d-4019-8e0b-bd480fdc8798"),
                            ColumnType = DPColumnTypeEnum.Int,
                            Name = "Key"
                        }
                    }
                },
                DPColumnsUniqueBlock = new ESQLMultiItemsContainer<DPColumnsUniqueBlock>()
                {
                    Mode = ModeForMultiItems.Full,
                    Announced = new List<DPColumnsUniqueBlock>()
                    {
                        new DPColumnsUniqueBlock()
                        {
                            ID = new Guid("2038343f-0d46-4534-8408-963ab18e763f"),
                            Columns = "Key"
                        }
                    }
                }
            };
            #endregion

            #region DPRelationTypeEnum
            var dpRelationTypeEnum = new DPEnumDescObject()
            {
                ID = new Guid("3fdb5f35-33f6-4356-8f65-f92da429191c"),
                DPObjectDescGenBlock = new DPObjectDescGenBlock()
                {
                    ID = new Guid("dbb5012a-958a-4272-8633-cdf04049fff4"),
                    Name = "DPRelationTypeEnum",
                    Kind = DPObjectKindEnum.Core
                },
                DPColumnDescBlock = new ESQLMultiItemsContainer<DPColumnDescBlock>()
                {
                    Mode = ModeForMultiItems.Full,
                    Announced = new List<DPColumnDescBlock>()
                    {
                        new DPColumnDescBlock()
                        {
                            ID = new Guid("c16e1093-6e36-4963-9a20-707429832b4d"),
                            ColumnType = DPColumnTypeEnum.String,
                            Name = "Value",
                            Length = 50
                        },
                        new DPColumnDescBlock()
                        {
                            ID = new Guid("0ce6d41d-1906-4d24-adc3-31f0922fd7cd"),
                            ColumnType = DPColumnTypeEnum.Int,
                            Name = "Key"
                        }
                    }
                },
                DPColumnsUniqueBlock = new ESQLMultiItemsContainer<DPColumnsUniqueBlock>()
                {
                    Mode = ModeForMultiItems.Full,
                    Announced = new List<DPColumnsUniqueBlock>()
                    {
                        new DPColumnsUniqueBlock()
                        {
                            ID = new Guid("f3e6e16c-6dc0-4056-aab2-6176e17c7ab4"),
                            Columns = "Key"
                        }
                    }
                }
            };
            #endregion

            CoreEnumInfos = new List<DPEnumDescObject>()
            {
                dpBlockInObjectTypeEnum,
                dpColumnTypeEnum,
                dpObjectKindEnum,
                dpRelationTypeEnum
            };
        }

        private static void InitCoreBlockInfos()
        {
            #region DPObjectDescGenBlock
            var dpObjectDescGenBlock = new DPBlockDescObject()
            {
                ID = new Guid("c5cf5513-9766-4cc6-84a0-b9a4717e36c2"),
                DPObjectDescGenBlock = new DPObjectDescGenBlock()
                {
                    ID = new Guid("3a911afb-9c07-4cf8-99b5-0ba02c4eb3f0"),
                    Name = "DPObjectDescGenBlock",
                    Kind = DPObjectKindEnum.Core
                },
                DPColumnDescBlock = new ESQLMultiItemsContainer<DPColumnDescBlock>()
                {
                    Mode = ModeForMultiItems.Full,
                    Announced = new List<DPColumnDescBlock>()
                    {
                        new DPColumnDescBlock()
                        {
                            Name = "ID",
                            ColumnType = DPColumnTypeEnum.GUID,
                            AllowNull = false
                        },
                        new DPColumnDescBlock()
                        {
                            Name = "ObjectID",
                            ColumnType = DPColumnTypeEnum.GUID,
                            AllowNull = false
                        },
                        new DPColumnDescBlock()
                        {
                            ID = new Guid("2a8e6b99-37ec-45dd-8dd1-c6163e56fb36"),
                            ColumnType = DPColumnTypeEnum.String,
                            Name = "Name",
                            AllowNull = false,
                            Length = 100
                        },
                        new DPColumnDescBlock()
                        {
                            ID = new Guid("d6f1c72a-42c3-42a1-ac44-b5d5ada561a4"),
                            ColumnType = DPColumnTypeEnum.String,
                            Name = "DisplayValue",
                            AllowNull = true,
                            Length = 500
                        }
                    }
                }
            };
            #endregion

            #region DPColumnDescBlock
            var dpColumnDescBlock = new DPBlockDescObject()
            {
                ID = new Guid("ce754889-4efb-4281-ad1f-14d710b30007"),
                DPObjectDescGenBlock = new DPObjectDescGenBlock()
                {
                    ID = new Guid("7725e3cb-e831-4ddc-86d8-c71bea59f9d7"),
                    Name = "DPColumnDescBlock",
                    Kind = DPObjectKindEnum.Core
                },
                DPColumnDescBlock = new ESQLMultiItemsContainer<DPColumnDescBlock>()
                {
                    Mode = ModeForMultiItems.Full,
                    Announced = new List<DPColumnDescBlock>()
                    {
                        new DPColumnDescBlock()
                        {
                            Name = "ID",
                            ColumnType = DPColumnTypeEnum.GUID,
                            AllowNull = false
                        },
                        new DPColumnDescBlock()
                        {
                            Name = "ObjectID",
                            ColumnType = DPColumnTypeEnum.GUID,
                            AllowNull = false
                        },
                        new DPColumnDescBlock()
                        {
                            ID = new Guid("203f9137-b7e0-46a5-b12b-551dd4493c67"),
                            ColumnType = DPColumnTypeEnum.String,
                            Name = "Name",
                            AllowNull = false,
                            Length = 100
                        },
                        new DPColumnDescBlock()
                        {
                            ID = new Guid("3a594944-e944-4da3-9203-fef22db78e58"),
                            ColumnType = DPColumnTypeEnum.Int,
                            Name = "Length",
                            AllowNull = true
                        },
                        new DPColumnDescBlock()
                        {
                            ID = new Guid("b5b6091d-7b2a-47f2-b5b6-b44499f2caf7"),
                            ColumnType = DPColumnTypeEnum.Int,
                            Name = "Precision",
                            AllowNull = true
                        },
                        new DPColumnDescBlock()
                        {
                            ID = new Guid("119437a8-7f00-4411-83ce-9990f769bbcf"),
                            ColumnType = DPColumnTypeEnum.Int,
                            Name = "Scale",
                            AllowNull = true
                        },
                        new DPColumnDescBlock()
                        {
                            ID = new Guid("858fa49d-7638-4a83-84a2-eed8dde5b4fa"),
                            ColumnType = DPColumnTypeEnum.Bool,
                            Name = "AllowNull",
                            AllowNull = true
                        },
                        new DPColumnDescBlock()
                        {
                            ID = new Guid("006134f1-7929-49e6-b51f-9647ab0b12f2"),
                            ColumnType = DPColumnTypeEnum.String,
                            Name = "DefaultValue",
                            AllowNull = true,
                            Length = 100
                        },
                        new DPColumnDescBlock()
                        {
                            ID = new Guid("86b96626-6d67-4afb-9616-583bd9ae0934"),
                            ColumnType = DPColumnTypeEnum.Int,
                            Name = "ColumnType",
                            AllowNull = false,
                            EnumKey = new Guid("884bb1f2-9eaf-4fa1-8bc1-9057edf5b5d2"),
                            EnumType = new Guid("971d538a-1489-483a-bc84-86596ed0c51a")
                        }
                    }
                }
            };
            #endregion

            #region DPColumnsUniqueBlock
            var dpColumnsUniqueBlock = new DPBlockDescObject()
            {
                ID = new Guid("575f9a04-6b51-4c0c-84e3-b4c624ee1f81"),
                DPObjectDescGenBlock = new DPObjectDescGenBlock()
                {
                    ID = new Guid("38402309-6239-4e54-8640-ac70772b76bd"),
                    Name = "DPColumnsUniqueBlock",
                    Kind = DPObjectKindEnum.Core
                },
                DPColumnDescBlock = new ESQLMultiItemsContainer<DPColumnDescBlock>()
                {
                    Mode = ModeForMultiItems.Full,
                    Announced = new List<DPColumnDescBlock>()
                    {
                        new DPColumnDescBlock()
                        {
                            Name = "ID",
                            ColumnType = DPColumnTypeEnum.GUID,
                            AllowNull = false
                        },
                        new DPColumnDescBlock()
                        {
                            Name = "ObjectID",
                            ColumnType = DPColumnTypeEnum.GUID,
                            AllowNull = false
                        },
                        new DPColumnDescBlock()
                        {
                            ID = new Guid("836b5fdc-c995-46d6-9151-fd562bfada19"),
                            ColumnType = DPColumnTypeEnum.String,
                            Name = "Columns",
                            AllowNull = false,
                            Length = 300
                        }
                    }
                }
            };
            #endregion

            #region DPEntityInheritanceBlock
            var dpEntityInheritanceBlock = new DPBlockDescObject()
            {
                ID = new Guid("eeb499d0-4e20-41aa-8a24-9981c3cbf511"),
                DPObjectDescGenBlock = new DPObjectDescGenBlock()
                {
                    ID = new Guid("5b452ce3-cb10-4e7d-91e9-16c0fb569350"),
                    Name = "DPEntityInheritanceBlock",
                    Kind = DPObjectKindEnum.Core
                },
                DPColumnDescBlock = new ESQLMultiItemsContainer<DPColumnDescBlock>()
                {
                    Mode = ModeForMultiItems.Full,
                    Announced = new List<DPColumnDescBlock>()
                    {
                        new DPColumnDescBlock()
                        {
                            Name = "ID",
                            ColumnType = DPColumnTypeEnum.GUID,
                            AllowNull = false
                        },
                        new DPColumnDescBlock()
                        {
                            Name = "ObjectID",
                            ColumnType = DPColumnTypeEnum.GUID,
                            AllowNull = false
                        },
                        new DPColumnDescBlock()
                        {
                            Name = "BaseEntity",
                            ColumnType = DPColumnTypeEnum.GUID,
                            AllowNull = true
                        }
                    }
                }
            };
            #endregion

            #region DPBlockInEntityDescGenBlock
            var dpBlockInEntityDescGenBlock = new DPBlockDescObject()
            {
                ID = new Guid("8b781efd-a6e5-4d24-9456-ea4a8d5fa5c7"),
                DPObjectDescGenBlock = new DPObjectDescGenBlock()
                {
                    ID = new Guid("abd3be0e-3d54-4cde-a909-fccf7293c661"),
                    Name = "DPBlockInEntityDescGenBlock",
                    Kind = DPObjectKindEnum.Core
                },
                DPColumnDescBlock = new ESQLMultiItemsContainer<DPColumnDescBlock>()
                {
                    Mode = ModeForMultiItems.Full,
                    Announced = new List<DPColumnDescBlock>()
                    {
                        new DPColumnDescBlock()
                        {
                            Name = "ID",
                            ColumnType = DPColumnTypeEnum.GUID,
                            AllowNull = false
                        },
                        new DPColumnDescBlock()
                        {
                            Name = "ObjectID",
                            ColumnType = DPColumnTypeEnum.GUID,
                            AllowNull = false
                        },
                        new DPColumnDescBlock()
                        {
                            ID = new Guid("2881e628-2258-4de9-a9a6-1f5e62f476b5"),
                            ColumnType = DPColumnTypeEnum.Int,
                            Name = "RelationType",
                            AllowNull = false,
                            EnumKey = new Guid("d214fd3a-99a2-4446-ba90-dd1e863e56e1"),
                            EnumType = new Guid("5e8630a5-e51a-4717-b63e-92a176e2aa8e")
                        }
                    }
                }
            };
            #endregion

            #region DPRelationGenBlock
            var dpRelationGenBlock = new DPBlockDescObject()
            {
                ID = new Guid("35cb012f-9ef5-43b8-b1e1-84f1f6b8cfed"),
                DPObjectDescGenBlock = new DPObjectDescGenBlock()
                {
                    ID = new Guid("5786ba46-1374-475a-baef-446954feea3f"),
                    Name = "DPRelationGenBlock",
                    Kind = DPObjectKindEnum.Core
                },
                DPColumnDescBlock = new ESQLMultiItemsContainer<DPColumnDescBlock>()
                {
                    Mode = ModeForMultiItems.Full,
                    Announced = new List<DPColumnDescBlock>()
                    {
                        new DPColumnDescBlock()
                        {
                            Name = "ID",
                            ColumnType = DPColumnTypeEnum.GUID,
                            AllowNull = false
                        },
                        new DPColumnDescBlock()
                        {
                            Name = "ObjectID",
                            ColumnType = DPColumnTypeEnum.GUID,
                            AllowNull = false
                        },
                        new DPColumnDescBlock()
                        {
                            ID = new Guid("002d22b0-2154-424a-b813-611178ed5864"),
                            ColumnType = DPColumnTypeEnum.String,
                            Name = "ObjectNameLeft",
                            AllowNull = false,
                            Length = 100
                        },
                        new DPColumnDescBlock()
                        {
                            ID = new Guid("94556eda-f8f5-4d5a-a1fc-ae4e0ac15cb5"),
                            ColumnType = DPColumnTypeEnum.String,
                            Name = "RelationNameLeft",
                            AllowNull = false,
                            Length = 100
                        },
                        new DPColumnDescBlock()
                        {
                            ID = new Guid("eb9ac69d-9198-49fe-9b29-6899cabe6340"),
                            ColumnType = DPColumnTypeEnum.String,
                            Name = "RelationColumnNameLeft",
                            AllowNull = true,
                            Length = 100
                        },
                        new DPColumnDescBlock()
                        {
                            ID = new Guid("08744099-30bb-46a6-9e44-e46f475b204b"),
                            ColumnType = DPColumnTypeEnum.Int,
                            Name = "RelationColumnTypeLeft",
                            AllowNull = true
                        },
                        new DPColumnDescBlock()
                        {
                            ID = new Guid("428c0829-d296-4d94-9b9a-94a65745543d"),
                            ColumnType = DPColumnTypeEnum.String,
                            Name = "ObjectNameRight",
                            AllowNull = false,
                            Length = 100
                        },
                        new DPColumnDescBlock()
                        {
                            ID = new Guid("e00f6022-7d20-4a90-b698-be8dfbe18332"),
                            ColumnType = DPColumnTypeEnum.String,
                            Name = "RelationNameRight",
                            AllowNull = false,
                            Length = 100
                        },
                        new DPColumnDescBlock()
                        {
                            ID = new Guid("9dc7d697-5f56-457e-8040-ff96766f5d75"),
                            ColumnType = DPColumnTypeEnum.String,
                            Name = "RelationColumnNameRight",
                            AllowNull = true,
                            Length = 100
                        },
                        new DPColumnDescBlock()
                        {
                            ID = new Guid("acbacb32-5e1f-4bb7-a06b-c50f346da38c"),
                            ColumnType = DPColumnTypeEnum.Int,
                            Name = "RelationColumnTypeRight",
                            AllowNull = true
                        },
                        new DPColumnDescBlock()
                        {
                            ID = new Guid("002d22b0-2154-424a-b813-611178ed5864"),
                            ColumnType = DPColumnTypeEnum.String,
                            Name = "RelationTable",
                            AllowNull = true,
                            Length = 100
                        },
                        new DPColumnDescBlock()
                        {
                            ID = new Guid("4692e78b-002a-4f29-9d78-96739292b1d0"),
                            ColumnType = DPColumnTypeEnum.Int,
                            Name = "RelationType",
                            AllowNull = false,
                            EnumKey = new Guid("0ce6d41d-1906-4d24-adc3-31f0922fd7cd"),
                            EnumType = new Guid("3fdb5f35-33f6-4356-8f65-f92da429191c")
                        },
                        new DPColumnDescBlock()
                        {
                            ID = new Guid("df750be8-2df0-4497-82fe-97eff7bba2eb"),
                            ColumnType = DPColumnTypeEnum.Int,
                            Name = "Kind",
                            AllowNull = false,
                            EnumKey = new Guid("15d97f21-fd2d-4019-8e0b-bd480fdc8798"),
                            EnumType = new Guid("3c9d2fa6-99e3-472b-b493-3e4790597f98")
                        }
                    },
                },
                DPColumnsUniqueBlock = new ESQLMultiItemsContainer<DPColumnsUniqueBlock>()
                {
                    Mode = ModeForMultiItems.Full,
                    Announced = new List<DPColumnsUniqueBlock>()
                    {
                        new DPColumnsUniqueBlock()
                        {
                            ID = new Guid("a210fada-2da8-46f0-a7b1-35963e21cda0"),
                            Columns = "ObjectNameLeft, RelationNameRight"
                        },
                        new DPColumnsUniqueBlock()
                        {
                            ID = new Guid("0338d236-4cce-4019-b9b7-e14657dd81a8"),
                            Columns = "ObjectNameRight, RelationNameLeft"
                        }
                    }
                }
            };
            #endregion

            #region DPMigrationScriptsGenBlock
            var dpMigrationScriptsGenBlock = new DPBlockDescObject()
            {
                ID = new Guid("4502e21d-4d38-475d-88eb-3159d3b7c514"),
                DPObjectDescGenBlock = new DPObjectDescGenBlock()
                {
                    ID = new Guid("923c3122-0cc6-4c8a-9679-0017f3b7f59d"),
                    Name = "DPMigrationScriptsGenBlock",
                    Kind = DPObjectKindEnum.Core
                },
                DPColumnDescBlock = new ESQLMultiItemsContainer<DPColumnDescBlock>()
                {
                    Mode = ModeForMultiItems.Full,
                    Announced = new List<DPColumnDescBlock>()
                    {
                        new DPColumnDescBlock()
                        {
                            Name = "ID",
                            ColumnType = DPColumnTypeEnum.GUID,
                            AllowNull = false
                        },
                        new DPColumnDescBlock()
                        {
                            Name = "ObjectID",
                            ColumnType = DPColumnTypeEnum.GUID,
                            AllowNull = false
                        },
                        new DPColumnDescBlock()
                        {
                            ID = new Guid("86a27647-265b-4335-bd1d-13ca6eef1085"),
                            ColumnType = DPColumnTypeEnum.String,
                            Name = "Version",
                            AllowNull = false,
                            Length = 2
                        },
                        new DPColumnDescBlock()
                        {
                            ID = new Guid("bc843f74-2d6a-4bea-a76a-1d49c143f5cd"),
                            ColumnType = DPColumnTypeEnum.String,
                            Name = "Build",
                            AllowNull = false,
                            Length = 2
                        },
                        new DPColumnDescBlock()
                        {
                            ID = new Guid("9ee89343-8361-40b4-a2f8-f3fa30a8a8e7"),
                            ColumnType = DPColumnTypeEnum.String,
                            Name = "Number",
                            AllowNull = false,
                            Length = 4
                        },
                        new DPColumnDescBlock()
                        {
                            ID = new Guid("330fb7c4-f77d-4ef9-ba1b-993a1a798c04"),
                            ColumnType = DPColumnTypeEnum.String,
                            Name = "AppName",
                            AllowNull = false,
                            Length = 10
                        },
                        new DPColumnDescBlock()
                        {
                            ID = new Guid("f0b946a5-b663-4593-a91d-a3054998c8ba"),
                            ColumnType = DPColumnTypeEnum.String,
                            Name = "Name",
                            AllowNull = false,
                            Length = 30
                        },
                        new DPColumnDescBlock()
                        {
                            ID = new Guid("432abec1-71ff-4502-8316-1025aa368903"),
                            ColumnType = DPColumnTypeEnum.String,
                            Name = "Extention",
                            AllowNull = false,
                            Length = 5
                        },
                        new DPColumnDescBlock()
                        {
                            ID = new Guid("881b6f1d-f465-4a00-99ee-3ba91347bd61"),
                            ColumnType = DPColumnTypeEnum.String,
                            Name = "FilePath",
                            AllowNull = false,
                            Length = 255
                        }
                    },
                },
                DPColumnsUniqueBlock = new ESQLMultiItemsContainer<DPColumnsUniqueBlock>()
                {
                    Mode = ModeForMultiItems.Full,
                    Announced = new List<DPColumnsUniqueBlock>()
                    {
                        new DPColumnsUniqueBlock()
                        {
                            ID = new Guid("3b9dbbcb-2739-451e-b08f-19e68fc566f1"),
                            Columns = "Version, Build, Number, AppName, Name, Extention"
                        }
                    }
                }
            };
            #endregion

            CoreBlockInfos = new List<DPBlockDescObject>()
            {
                dpObjectDescGenBlock,
                dpColumnDescBlock,
                dpColumnsUniqueBlock,
                dpEntityInheritanceBlock,
                dpBlockInEntityDescGenBlock,
                dpRelationGenBlock,
                dpMigrationScriptsGenBlock
            };
        }

        private static void InitCoreEntityInfos()
        {
            #region dpObjectDescObject
            var dpObjectDescObject = new DPEntityDescObject()
            {
                ID = new Guid("2a30fc41-144d-45a8-b74a-e4ca528fc81c"),
                DPObjectDescGenBlock = new DPObjectDescGenBlock()
                {
                    ID = new Guid("09b4eda6-96d0-4d47-b8bd-d7879a45ea72"),
                    Name = "DPObjectDescObject",
                    Kind = DPObjectKindEnum.Core
                },
                DPBlockInEntityDescGenBlock = new ESQLMultiItemsContainer<DPBlockInEntityDescGenBlock>()
                {
                    Mode = ModeForMultiItems.Full,
                    Announced = new List<DPBlockInEntityDescGenBlock>()
                    {
                        new DPBlockInEntityDescGenBlock()
                        {
                            ID = new Guid("c0074af4-a729-49d2-9fe2-f62a54cd6ac0"),
                            RelationType = DPBlockInObjectTypeEnum.SingleMandatory,
                            DPBlockDescObject = new Guid("c5cf5513-9766-4cc6-84a0-b9a4717e36c2")
                        },
                        new DPBlockInEntityDescGenBlock()
                        {
                            ID = new Guid("4b1500ca-0400-4d2e-ab25-bd77b590ee95"),
                            RelationType = DPBlockInObjectTypeEnum.MultiOptional,
                            DPBlockDescObject = new Guid("ce754889-4efb-4281-ad1f-14d710b30007")
                        },
                        new DPBlockInEntityDescGenBlock()
                        {
                            ID = new Guid("29e7062c-1669-40e6-af24-adf132742a98"),
                            RelationType = DPBlockInObjectTypeEnum.MultiOptional,
                            DPBlockDescObject = new Guid("575f9a04-6b51-4c0c-84e3-b4c624ee1f81")
                        }
                    }
                }
            };
            #endregion

            #region DPEntityDescObject
            var dpEntityDescObject = new DPEntityDescObject()
            {
                ID = new Guid("c60e25e6-2e6e-4d0b-8976-7b0aeb3d41d5"),
                DPObjectDescGenBlock = new DPObjectDescGenBlock()
                {
                    ID = new Guid("3dd9bba0-102e-4f5a-8161-383fb2ab8a35"),
                    Name = "DPEntityDescObject",
                    Kind = DPObjectKindEnum.Core
                },
                DPEntityInheritanceBlock = new DPEntityInheritanceBlock()
                {
                    ID = new Guid("710b1d0b-9343-4739-8126-ab4baefe5763"),
                    BaseEntity = new Guid("2a30fc41-144d-45a8-b74a-e4ca528fc81c"),
                },
                DPBlockInEntityDescGenBlock = new ESQLMultiItemsContainer<DPBlockInEntityDescGenBlock>()
                {
                    Mode = ModeForMultiItems.Full,
                    Announced = new List<DPBlockInEntityDescGenBlock>()
                    {
                        new DPBlockInEntityDescGenBlock()
                        {
                            ID = new Guid("15399025-98be-4cf1-9761-413b5fde515c"),
                            RelationType = DPBlockInObjectTypeEnum.SingleOptional,
                            DPBlockDescObject = new Guid("eeb499d0-4e20-41aa-8a24-9981c3cbf511")
                        },
                        new DPBlockInEntityDescGenBlock()
                        {
                            ID = new Guid("5c9c45e1-6793-418a-a139-fffa3dd386d9"),
                            RelationType = DPBlockInObjectTypeEnum.MultiOptional,
                            DPBlockDescObject = new Guid("8b781efd-a6e5-4d24-9456-ea4a8d5fa5c7")
                        }
                    }
                }
            };
            #endregion

            #region DPBlockDescObject
            var dpBlockDescObject = new DPEntityDescObject()
            {
                ID = new Guid("cee041ff-53d1-46cc-b2ae-d9cb4db0e577"),
                DPObjectDescGenBlock = new DPObjectDescGenBlock()
                {
                    ID = new Guid("a40d4fd0-1bf7-4d0b-a68c-d98f873b326d"),
                    Name = "DPBlockDescObject",
                    Kind = DPObjectKindEnum.Core
                },
                DPEntityInheritanceBlock = new DPEntityInheritanceBlock()
                {
                    ID = new Guid("61b4c43f-cc9b-460e-80f9-9f2a7f4f7ca9"),
                    BaseEntity = new Guid("2a30fc41-144d-45a8-b74a-e4ca528fc81c"),
                }
            };
            #endregion

            #region DPEnumDescObject
            var dpEnumDescObject = new DPEntityDescObject()
            {
                ID = new Guid("baa62331-fa09-47c6-8d33-f2b25ec29bf1"),
                DPObjectDescGenBlock = new DPObjectDescGenBlock()
                {
                    ID = new Guid("415443b9-16dd-46dd-873b-06266a986c0b"),
                    Name = "DPEnumDescObject",
                    Kind = DPObjectKindEnum.Core
                },
                DPEntityInheritanceBlock = new DPEntityInheritanceBlock()
                {
                    ID = new Guid("408785e8-7e42-4e7f-b60a-d5c686911612"),
                    BaseEntity = new Guid("2a30fc41-144d-45a8-b74a-e4ca528fc81c"),
                }
            };
            #endregion

            #region DPRelationObject
            var dpRelationObject = new DPEntityDescObject()
            {
                ID = new Guid("9fec9aab-c0d9-4453-90e7-06b023aa6faf"),
                DPObjectDescGenBlock = new DPObjectDescGenBlock()
                {
                    ID = new Guid("f440497f-f9b9-4b25-a558-e7bdd2354683"),
                    Name = "DPRelationObject",
                    Kind = DPObjectKindEnum.Core
                },
                DPBlockInEntityDescGenBlock = new ESQLMultiItemsContainer<DPBlockInEntityDescGenBlock>()
                {
                    Mode = ModeForMultiItems.Full,
                    Announced = new List<DPBlockInEntityDescGenBlock>()
                    {
                        new DPBlockInEntityDescGenBlock()
                        {
                            ID = new Guid("3567c5d3-c419-4507-a404-d71924e2cda6"),
                            RelationType = DPBlockInObjectTypeEnum.SingleMandatory,
                            DPBlockDescObject = new Guid("35cb012f-9ef5-43b8-b1e1-84f1f6b8cfed")
                        }
                    }
                }
            };
            #endregion

            #region DPMigrationScriptsObject
            var dpMigrationScriptsObject = new DPEntityDescObject()
            {
                ID = new Guid("0f4a01ba-427d-41ca-9f98-5dddadbd25d6"),
                DPObjectDescGenBlock = new DPObjectDescGenBlock()
                {
                    ID = new Guid("f440497f-f9b9-4b25-a558-e7bdd2354683"),
                    Name = "DPMigrationScriptsObject",
                    Kind = DPObjectKindEnum.Core
                },
                DPBlockInEntityDescGenBlock = new ESQLMultiItemsContainer<DPBlockInEntityDescGenBlock>()
                {
                    Mode = ModeForMultiItems.Full,
                    Announced = new List<DPBlockInEntityDescGenBlock>()
                    {
                        new DPBlockInEntityDescGenBlock()
                        {
                            ID = new Guid("6881bd56-1753-472d-9699-a1d640a1f53a"),
                            RelationType = DPBlockInObjectTypeEnum.SingleMandatory,
                            DPBlockDescObject = new Guid("4502e21d-4d38-475d-88eb-3159d3b7c514")
                        }
                    }
                }
            };
            #endregion

            CoreEntityInfos = new List<DPEntityDescObject>()
            {
                dpObjectDescObject,
                dpEntityDescObject,
                dpBlockDescObject,
                dpEnumDescObject,
                dpRelationObject,
                dpMigrationScriptsObject
            };
        }
    }
}
