using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;

namespace IV.DX.Persistence.CoreData
{
    internal static class DPEntityDescObjectItems
    {
        public static IList<DPEntityDescObject> Items { get; private set; }

        static DPEntityDescObjectItems()
        {
            Items = new List<DPEntityDescObject>()
            {
                #region DPObjectDescObject
                new DPEntityDescObject()
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
                            },
                        }
                    }
                },
                #endregion
                #region DPEntityDescObject
                new DPEntityDescObject()
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
                },
                #endregion
                #region DPBlockDescObject
                new DPEntityDescObject()
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
                },
                #endregion
                #region DPEnumDescObject
                new DPEntityDescObject()
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
                },
                #endregion
                #region DPRelationObject
                new DPEntityDescObject()
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
                },
                #endregion
                #region DPMigrationScriptsObject
                new DPEntityDescObject()
                {
                    ID = new Guid("0f4a01ba-427d-41ca-9f98-5dddadbd25d6"),
                    DPObjectDescGenBlock = new DPObjectDescGenBlock()
                    {
                        ID = new Guid("e4b329e3-9282-40ce-91ac-58f2e8c5433c"),
                        Name = "DPMigrationScriptsObject",
                        Kind = DPObjectKindEnum.Core
                    },
                    DPBlockInEntityDescGenBlock = new ESQLMultiItemsContainer<DPBlockInEntityDescGenBlock>()
                    {
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
                },
                #endregion
            };
        }
    }
}
