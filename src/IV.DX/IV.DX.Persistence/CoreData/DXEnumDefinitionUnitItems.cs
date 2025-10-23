using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;

namespace IV.DX.Persistence.CoreData
{
    internal static class DXEnumDefinitionUnitItems
    {
        public static IList<DXEnumDefinitionUnit> Items { get; private set; }

        static DXEnumDefinitionUnitItems()
        {
            Items = new List<DXEnumDefinitionUnit>()
            {
                #region DXElementInUnitTypeEnum
                new DXEnumDefinitionUnit()
                {
                    ID = new Guid("5e8630a5-e51a-4717-b63e-92a176e2aa8e"),
                    DXObjectDefinitionMainElement= new DXObjectDefinitionMainElement()
                    {
                        ID = new Guid("59b115c5-cc8b-4c88-b1e0-d94af8745e99"),
                        Name = "DXElementInUnitTypeEnum",
                        Kind = DXObjectKindEnum.Core
                    },
                    DXColumnDefinitionElement = new DXMultiElementsContainer<DXColumnDefinitionElement>()
                    {
                        Announced = new HashSet<DXColumnDefinitionElement>()
                        {
                            new DXColumnDefinitionElement()
                            {
                                ID = new Guid("72d914ae-c902-45dd-b405-fb12d5021597"),
                                ColumnType = DXColumnTypeEnum.String,
                                Name = "Value",
                                Length = 50,
                                AllowNull = false,
                                DefaultValue = null
                            },
                            new DXColumnDefinitionElement()
                            {
                                ID = new Guid("d214fd3a-99a2-4446-ba90-dd1e863e56e1"),
                                ColumnType = DXColumnTypeEnum.Int,
                                Name = "Key",
                                AllowNull = false,
                                DefaultValue = null
                            }
                        }
                    },
                    DXUniqueColumnsElement = new DXMultiElementsContainer<DXUniqueColumnsElement>()
                    {
                        Announced = new HashSet<DXUniqueColumnsElement>()
                        {
                            new DXUniqueColumnsElement()
                            {
                                ID = new Guid("44892f0b-5f87-4373-b3c2-a7eac4a41ffd"),
                                Columns = "Key"
                            }
                        }
                    }
                },
	            #endregion  
                #region DXColumnTypeEnum
                new DXEnumDefinitionUnit()
                {
                    ID = new Guid("971d538a-1489-483a-bc84-86596ed0c51a"),
                    DXObjectDefinitionMainElement= new DXObjectDefinitionMainElement()
                    {
                        ID = new Guid("261cfd33-65cb-4f36-8712-be15e6621cc0"),
                        Name = "DXColumnTypeEnum",
                        Kind = DXObjectKindEnum.Core
                    },
                    DXColumnDefinitionElement = new DXMultiElementsContainer<DXColumnDefinitionElement>()
                    {
                        Announced = new HashSet<DXColumnDefinitionElement>()
                        {
                            new DXColumnDefinitionElement()
                            {
                                ID = new Guid("940d5067-9271-4dc2-9ae0-b1b83c519e11"),
                                ColumnType = DXColumnTypeEnum.String,
                                Name = "Value",
                                Length = 50,
                                AllowNull = false,
                                DefaultValue = null
                            },
                            new DXColumnDefinitionElement()
                            {
                                ID = new Guid("884bb1f2-9eaf-4fa1-8bc1-9057edf5b5d2"),
                                ColumnType = DXColumnTypeEnum.Int,
                                Name = "Key",
                                AllowNull = false,
                                DefaultValue = null
                            }
                        }
                    },
                    DXUniqueColumnsElement = new DXMultiElementsContainer<DXUniqueColumnsElement>()
                    {
                        Announced = new HashSet<DXUniqueColumnsElement>()
                        {
                            new DXUniqueColumnsElement()
                            {
                                ID = new Guid("ff9aab92-03c6-4c38-8147-08c77e694fe9"),
                                Columns = "Key"
                            }
                        }
                    }
                },
	            #endregion  
                #region DXObjectKindEnum
                new DXEnumDefinitionUnit()
                {
                    ID = new Guid("3c9d2fa6-99e3-472b-b493-3e4790597f98"),
                    DXObjectDefinitionMainElement= new DXObjectDefinitionMainElement()
                    {
                        ID = new Guid("179f9be7-dc54-4ac7-a9c5-ea50c7524752"),
                        Name = "DXObjectKindEnum",
                        Kind = DXObjectKindEnum.Core
                    },
                    DXColumnDefinitionElement = new DXMultiElementsContainer<DXColumnDefinitionElement>()
                    {
                        Announced = new HashSet<DXColumnDefinitionElement>()
                        {
                            new DXColumnDefinitionElement()
                            {
                                ID = new Guid("6212d559-2cf3-4341-a517-89f3a57abe78"),
                                ColumnType = DXColumnTypeEnum.String,
                                Name = "Value",
                                Length = 50,
                                AllowNull = false,
                                DefaultValue = null
                            },
                            new DXColumnDefinitionElement()
                            {
                                ID = new Guid("15d97f21-fd2d-4019-8e0b-bd480fdc8798"),
                                ColumnType = DXColumnTypeEnum.Int,
                                Name = "Key",
                                AllowNull = false,
                                DefaultValue = null
                            }
                        }
                    },
                    DXUniqueColumnsElement = new DXMultiElementsContainer<DXUniqueColumnsElement>()
                    {
                        Announced = new HashSet<DXUniqueColumnsElement>()
                        {
                            new DXUniqueColumnsElement()
                            {
                                ID = new Guid("2038343f-0d46-4534-8408-963ab18e763f"),
                                Columns = "Key"
                            }
                        }
                    }
                },
	            #endregion  
                #region DXRelationTypeEnum
                new DXEnumDefinitionUnit()
                {
                    ID = new Guid("3fdb5f35-33f6-4356-8f65-f92da429191c"),
                    DXObjectDefinitionMainElement= new DXObjectDefinitionMainElement()
                    {
                        ID = new Guid("dbb5012a-958a-4272-8633-cdf04049fff4"),
                        Name = "DXRelationTypeEnum",
                        Kind = DXObjectKindEnum.Core
                    },
                    DXColumnDefinitionElement = new DXMultiElementsContainer<DXColumnDefinitionElement>()
                    {
                        Announced = new HashSet<DXColumnDefinitionElement>()
                        {
                            new DXColumnDefinitionElement()
                            {
                                ID = new Guid("c16e1093-6e36-4963-9a20-707429832b4d"),
                                ColumnType = DXColumnTypeEnum.String,
                                Name = "Value",
                                Length = 50,
                                AllowNull = false,
                                DefaultValue = null
                            },
                            new DXColumnDefinitionElement()
                            {
                                ID = new Guid("0ce6d41d-1906-4d24-adc3-31f0922fd7cd"),
                                ColumnType = DXColumnTypeEnum.Int,
                                Name = "Key",
                                AllowNull = false,
                                DefaultValue = null
                            }
                        }
                    },
                    DXUniqueColumnsElement = new DXMultiElementsContainer<DXUniqueColumnsElement>()
                    {
                        Announced = new HashSet<DXUniqueColumnsElement>()
                        {
                            new DXUniqueColumnsElement()
                            {
                                ID = new Guid("f3e6e16c-6dc0-4056-aab2-6176e17c7ab4"),
                                Columns = "Key"
                            }
                        }
                    }
                },
	            #endregion  
            };
        }
    }
}
