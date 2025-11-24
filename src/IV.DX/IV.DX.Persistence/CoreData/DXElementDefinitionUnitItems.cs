using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;

namespace IV.DX.Persistence.CoreData
{
    internal static class DXElementDefinitionUnitItems
    {
        public static IList<DXElementDefinitionUnit> Items { get; private set; }

        static DXElementDefinitionUnitItems()
        {
            Items = new List<DXElementDefinitionUnit>()
            {
                #region DXObjectDefinitionMainElement
                new DXElementDefinitionUnit()
                {
                    ID = new Guid("c5cf5513-9766-4cc6-84a0-b9a4717e36c2"),
                    DXObjectDefinitionMainElement = new DXObjectDefinitionMainElement()
                    {
                        ID = new Guid("3a911afb-9c07-4cf8-99b5-0ba02c4eb3f0"),
                        Name = "DXObjectDefinitionMainElement",
                        Kind = DXObjectKindEnum.Core
                    },
                    DXColumnDefinitionElement = new DXMultiElementsContainer<DXColumnDefinitionElement>()
                    {
                        Announced = new HashSet<DXColumnDefinitionElement>()
                        {
                            new DXColumnDefinitionElement()
                            {
                                ID = new Guid("2a8e6b99-37ec-45dd-8dd1-c6163e56fb36"),
                                ColumnType = DXColumnTypeEnum.String,
                                Name = "Name",
                                AllowNull = false,
                                DefaultValue = null,
                                Length = 100
                            },
                            new DXColumnDefinitionElement()
                            {
                                ID = new Guid("d6f1c72a-42c3-42a1-ac44-b5d5ada561a4"),
                                ColumnType = DXColumnTypeEnum.String,
                                Name = "DisplayValue",
                                AllowNull = false,
                                DefaultValue = null,
                                Length = 500
                            }
                        }
                    }
                },
                #endregion
                #region DXColumnDefinitionElement
                new DXElementDefinitionUnit()
                {
                    ID = new Guid("ce754889-4efb-4281-ad1f-14d710b30007"),
                    DXObjectDefinitionMainElement = new DXObjectDefinitionMainElement()
                    {
                        ID = new Guid("7725e3cb-e831-4ddc-86d8-c71bea59f9d7"),
                        Name = "DXColumnDefinitionElement",
                        Kind = DXObjectKindEnum.Core
                    },
                    DXColumnDefinitionElement = new DXMultiElementsContainer<DXColumnDefinitionElement>()
                    {
                        Announced = new HashSet<DXColumnDefinitionElement>()
                        {
                            new DXColumnDefinitionElement()
                            {
                                ID = new Guid("203f9137-b7e0-46a5-b12b-551dd4493c67"),
                                ColumnType = DXColumnTypeEnum.String,
                                Name = "Name",
                                AllowNull = false,
                                DefaultValue = null,
                                Length = 100
                            },
                            new DXColumnDefinitionElement()
                            {
                                ID = new Guid("3a594944-e944-4da3-9203-fef22db78e58"),
                                ColumnType = DXColumnTypeEnum.Int,
                                Name = "Length",
                                AllowNull = true,
                                DefaultValue = null
                            },
                            new DXColumnDefinitionElement()
                            {
                                ID = new Guid("3a594944-e944-4da3-9203-fef22db78e58"),
                                ColumnType = DXColumnTypeEnum.Int,
                                Name = "Precision",
                                AllowNull = true,
                                DefaultValue = null
                            },
                            new DXColumnDefinitionElement()
                            {
                                ID = new Guid("119437a8-7f00-4411-83ce-9990f769bbcf"),
                                ColumnType = DXColumnTypeEnum.Int,
                                Name = "Scale",
                                AllowNull = true,
                                DefaultValue = null
                            },
                            new DXColumnDefinitionElement()
                            {
                                ID = new Guid("858fa49d-7638-4a83-84a2-eed8dde5b4fa"),
                                ColumnType = DXColumnTypeEnum.Bool,
                                Name = "AllowNull",
                                AllowNull = true,
                                DefaultValue = null
                            },
                            new DXColumnDefinitionElement()
                            {
                                ID = new Guid("006134f1-7929-49e6-b51f-9647ab0b12f2"),
                                ColumnType = DXColumnTypeEnum.String,
                                Name = "DefaultValue",
                                AllowNull = true,
                                DefaultValue = null,
                                Length = 100
                            }
                        }
                    }
                },
                #endregion
                #region DXUniqueColumnsElement
                new DXElementDefinitionUnit()
                {
                    ID = new Guid("575f9a04-6b51-4c0c-84e3-b4c624ee1f81"),
                    DXObjectDefinitionMainElement = new DXObjectDefinitionMainElement()
                    {
                        ID = new Guid("38402309-6239-4e54-8640-ac70772b76bd"),
                        Name = "DXUniqueColumnsElement",
                        Kind = DXObjectKindEnum.Core
                    },
                    DXColumnDefinitionElement = new DXMultiElementsContainer<DXColumnDefinitionElement>()
                    {
                        Announced = new HashSet<DXColumnDefinitionElement>()
                        {
                            new DXColumnDefinitionElement()
                            {
                                ID = new Guid("836b5fdc-c995-46d6-9151-fd562bfada19"),
                                ColumnType = DXColumnTypeEnum.String,
                                Name = "Columns",
                                AllowNull = false,
                                DefaultValue = null,
                                Length = 100
                            }
                        }
                    }
                },
                #endregion
                #region DXObjectEnumElements
                new DXElementDefinitionUnit()
                {
                    ID = new Guid("30b46356-a7e4-43bd-ace2-d4d9b4a87c51"),
                    DXObjectDefinitionMainElement = new DXObjectDefinitionMainElement()
                    {
                        ID = new Guid("405c1a7c-5248-4432-acd6-37c9b1f7d3ba"),
                        Name = "DXObjectEnumElement",
                        Kind = DXObjectKindEnum.Core
                    },
                    DXColumnDefinitionElement = new DXMultiElementsContainer<DXColumnDefinitionElement>()
                    {
                        Mode = MultiElementsMode.Full,
                        Announced = new HashSet<DXColumnDefinitionElement>()
                        {
                            new DXColumnDefinitionElement()
                            {
                                Name = "ID",
                                ColumnType = DXColumnTypeEnum.GUID,
                                AllowNull = false
                            },
                            new DXColumnDefinitionElement()
                            {
                                Name = "DXUnitID",
                                ColumnType = DXColumnTypeEnum.GUID,
                                AllowNull = false
                            },
                            new DXColumnDefinitionElement()
                            {
                                ID = new Guid("836b5fdc-c995-46d6-9151-fd562bfada19"),
                                ColumnType = DXColumnTypeEnum.String,
                                Name = "Name",
                                AllowNull = false,
                                Length = 100
                            }
                        }
                    }
                },
                #endregion
                #region DXUniqueColumnsElement
                new DXElementDefinitionUnit()
                {
                    ID = new Guid("eeb499d0-4e20-41aa-8a24-9981c3cbf511"),
                    DXObjectDefinitionMainElement = new DXObjectDefinitionMainElement()
                    {
                        ID = new Guid("5b452ce3-cb10-4e7d-91e9-16c0fb569350"),
                        Name = "DXUnitInheritanceElement",
                        Kind = DXObjectKindEnum.Core
                    }
                },
                #endregion
                #region DXElementInUnitDefinitionElement
                new DXElementDefinitionUnit()
                {
                    ID = new Guid("8b781efd-a6e5-4d24-9456-ea4a8d5fa5c7"),
                    DXObjectDefinitionMainElement = new DXObjectDefinitionMainElement()
                    {
                        ID = new Guid("abd3be0e-3d54-4cde-a909-fccf7293c661"),
                        Name = "DXElementInUnitDefinitionElement",
                        Kind = DXObjectKindEnum.Core
                    }
                },
                #endregion
                #region DXRelationDefinitionMainElement
                new DXElementDefinitionUnit()
                {
                    ID = new Guid("35cb012f-9ef5-43b8-b1e1-84f1f6b8cfed"),
                    DXObjectDefinitionMainElement = new DXObjectDefinitionMainElement()
                    {
                        ID = new Guid("5786ba46-1374-475a-baef-446954feea3f"),
                        Name = "DXRelationDefinitionMainElement",
                        Kind = DXObjectKindEnum.Core
                    },
                    DXColumnDefinitionElement = new DXMultiElementsContainer<DXColumnDefinitionElement>()
                    {
                        Announced = new HashSet<DXColumnDefinitionElement>()
                        {
                            new DXColumnDefinitionElement()
                            {
                                ID = new Guid("002d22b0-2154-424a-b813-611178ed5864"),
                                ColumnType = DXColumnTypeEnum.String,
                                Name = "ObjectNameLeft",
                                AllowNull = false,
                                DefaultValue = null,
                                Length = 100
                            },
                            new DXColumnDefinitionElement()
                            {
                                ID = new Guid("94556eda-f8f5-4d5a-a1fc-ae4e0ac15cb5"),
                                ColumnType = DXColumnTypeEnum.String,
                                Name = "RelationNameLeft",
                                AllowNull = false,
                                DefaultValue = null,
                                Length = 100
                            },
                            new DXColumnDefinitionElement()
                            {
                                ID = new Guid("eb9ac69d-9198-49fe-9b29-6899cabe6340"),
                                ColumnType = DXColumnTypeEnum.String,
                                Name = "RelationColumnNameLeft",
                                AllowNull = true,
                                DefaultValue = null,
                                Length = 100
                            },
                            new DXColumnDefinitionElement()
                            {
                                ID = new Guid("08744099-30bb-46a6-9e44-e46f475b204b"),
                                ColumnType = DXColumnTypeEnum.Int,
                                Name = "RelationColumnTypeLeft",
                                AllowNull = true,
                                DefaultValue = null
                            },
                            new DXColumnDefinitionElement()
                            {
                                ID = new Guid("428c0829-d296-4d94-9b9a-94a65745543d"),
                                ColumnType = DXColumnTypeEnum.String,
                                Name = "ObjectNameRight",
                                AllowNull = false,
                                DefaultValue = null,
                                Length = 100
                            },
                            new DXColumnDefinitionElement()
                            {
                                ID = new Guid("e00f6022-7d20-4a90-b698-be8dfbe18332"),
                                ColumnType = DXColumnTypeEnum.String,
                                Name = "RelationNameRight",
                                AllowNull = false,
                                DefaultValue = null,
                                Length = 100
                            },
                            new DXColumnDefinitionElement()
                            {
                                ID = new Guid("9dc7d697-5f56-457e-8040-ff96766f5d75"),
                                ColumnType = DXColumnTypeEnum.String,
                                Name = "RelationColumnNameRight",
                                AllowNull = true,
                                DefaultValue = null,
                                Length = 100
                            },
                            new DXColumnDefinitionElement()
                            {
                                ID = new Guid("acbacb32-5e1f-4bb7-a06b-c50f346da38c"),
                                ColumnType = DXColumnTypeEnum.Int,
                                Name = "RelationColumnTypeRight",
                                AllowNull = true,
                                DefaultValue = null
                            },
                            new DXColumnDefinitionElement()
                            {
                                ID = new Guid("efce7a8d-892a-49d8-9f8f-0dec7cd96f19"),
                                ColumnType = DXColumnTypeEnum.String,
                                Name = "RelationTable",
                                AllowNull = true,
                                DefaultValue = null,
                                Length = 100
                            }
                        }
                    },
                    DXUniqueColumnsElement = new DXMultiElementsContainer<DXUniqueColumnsElement>()
                    {
                        Announced = new HashSet<DXUniqueColumnsElement>()
                        {
                            new DXUniqueColumnsElement()
                            {
                                ID = new Guid("a210fada-2da8-46f0-a7b1-35963e21cda0"),
                                Columns = "ObjectNameLeft, RelationNameRight"
                            },
                            new DXUniqueColumnsElement()
                            {
                                ID = new Guid("0338d236-4cce-4019-b9b7-e14657dd81a8"),
                                Columns = "ObjectNameRight, RelationNameLeft"
                            }
                        }
                    }
                },
                #endregion
                #region DXMigrationScriptsMainElement
                new DXElementDefinitionUnit()
                {
                    ID = new Guid("4502e21d-4d38-475d-88eb-3159d3b7c514"),
                    DXObjectDefinitionMainElement = new DXObjectDefinitionMainElement()
                    {
                        ID = new Guid("923c3122-0cc6-4c8a-9679-0017f3b7f59d"),
                        Name = "DXMigrationScriptsMainElement",
                        Kind = DXObjectKindEnum.Core
                    },
                    DXColumnDefinitionElement = new DXMultiElementsContainer<DXColumnDefinitionElement>()
                    {
                        Announced = new HashSet<DXColumnDefinitionElement>()
                        {
                            new DXColumnDefinitionElement()
                            {
                                ID = new Guid("86a27647-265b-4335-bd1d-13ca6eef1085"),
                                ColumnType = DXColumnTypeEnum.String,
                                Name = "Version",
                                AllowNull = false,
                                DefaultValue = null,
                                Length = 2
                            },
                            new DXColumnDefinitionElement()
                            {
                                ID = new Guid("bc843f74-2d6a-4bea-a76a-1d49c143f5cd"),
                                ColumnType = DXColumnTypeEnum.String,
                                Name = "Build",
                                AllowNull = false,
                                DefaultValue = null,
                                Length = 2
                            },
                            new DXColumnDefinitionElement()
                            {
                                ID = new Guid("9ee89343-8361-40b4-a2f8-f3fa30a8a8e7"),
                                ColumnType = DXColumnTypeEnum.String,
                                Name = "Number",
                                AllowNull = false,
                                DefaultValue = null,
                                Length = 4
                            },
                            new DXColumnDefinitionElement()
                            {
                                ID = new Guid("330fb7c4-f77d-4ef9-ba1b-993a1a798c04"),
                                ColumnType = DXColumnTypeEnum.String,
                                Name = "AppName",
                                AllowNull = false,
                                DefaultValue = null,
                                Length = 10
                            },
                            new DXColumnDefinitionElement()
                            {
                                ID = new Guid("432abec1-71ff-4502-8316-1025aa368903"),
                                ColumnType = DXColumnTypeEnum.String,
                                Name = "Name",
                                AllowNull = false,
                                DefaultValue = null,
                                Length = 30
                            },
                            new DXColumnDefinitionElement()
                            {
                                ID = new Guid("f0b946a5-b663-4593-a91d-a3054998c8ba"),
                                ColumnType = DXColumnTypeEnum.String,
                                Name = "Extention",
                                AllowNull = false,
                                DefaultValue = null,
                                Length = 5
                            },
                            new DXColumnDefinitionElement()
                            {
                                ID = new Guid("881b6f1d-f465-4a00-99ee-3ba91347bd61"),
                                ColumnType = DXColumnTypeEnum.String,
                                Name = "FilePath",
                                AllowNull = false,
                                DefaultValue = null,
                                Length = 255
                            }
                        }
                    },
                    DXUniqueColumnsElement = new DXMultiElementsContainer<DXUniqueColumnsElement>()
                    {
                        Announced = new HashSet<DXUniqueColumnsElement>()
                        {
                            new DXUniqueColumnsElement()
                            {
                                ID = new Guid("3b9dbbcb-2739-451e-b08f-19e68fc566f1"),
                                Columns = "Version, Build, Number, AppName, Name, Extention"
                            }
                        }
                    }
                },
                #endregion
            };
        }
    }
}
