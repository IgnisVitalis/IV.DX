using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;

namespace IV.DX.Persistence.CoreData
{
    internal static class DXRelationDefinitionUnitItems
    {
        public static IList<DXRelationDefinitionUnit> Items { get; private set; }

        static DXRelationDefinitionUnitItems()
        {
            Items = new List<DXRelationDefinitionUnit>()
            {
                #region 9020e4fa-3ce9-4b32-8849-cdabe0b6f707
                new DXRelationDefinitionUnit()
                {
                    ID = new Guid("9020e4fa-3ce9-4b32-8849-cdabe0b6f707"),
                    DPRelationGenBlock = new DPRelationGenBlock()
                    {
                        ID = new Guid("a0e9308c-7e20-4eff-add8-9ce21e1de13b"),
                        ObjectNameLeft = "DPObjectDescObject",
                        RelationNameLeft = "DPObjectDescObjectID",
                        ObjectNameRight = "DXUnitDefinitionMainElement",
                        RelationNameRight = "DXUnitDefinitionMainElement",
                        RelationTable = null,
                        RelationType = DPRelationTypeEnum.ZeroOneToZeroOne,
                        Kind = DXObjectKindEnum.Core
                    }
                },
                #endregion
                #region f802e73c-e512-4655-8067-94e80aea143c
                new DXRelationDefinitionUnit()
                {
                    ID = new Guid("f802e73c-e512-4655-8067-94e80aea143c"),
                    DPRelationGenBlock = new DPRelationGenBlock()
                    {
                        ID = new Guid("e7ec1df8-e169-406c-9323-52e5e6459c95"),
                        ObjectNameLeft = "DPObjectDescObject",
                        RelationNameLeft = "DPObjectDescObjectID",
                        ObjectNameRight = "DXColumnDefinitionElement",
                        RelationNameRight = "DXColumnDefinitionElement",
                        RelationTable = null,
                        RelationType = DPRelationTypeEnum.ZeroOneToMany,
                        Kind = DXObjectKindEnum.Core
                    }
                },
                #endregion
                #region bedc1f2b-2b9b-404f-89e5-5179bd87f60f
                new DXRelationDefinitionUnit()
                {
                    ID = new Guid("bedc1f2b-2b9b-404f-89e5-5179bd87f60f"),
                    DPRelationGenBlock = new DPRelationGenBlock()
                    {
                        ID = new Guid("afd2a155-e21e-44b3-b719-139b7a7deefc"),
                        ObjectNameLeft = "DPObjectDescObject",
                        RelationNameLeft = "DPObjectDescObjectID",
                        ObjectNameRight = "DXUniqueColumnsElement",
                        RelationNameRight = "DXUniqueColumnsElement",
                        RelationTable = null,
                        RelationType = DPRelationTypeEnum.ZeroOneToMany,
                        Kind = DXObjectKindEnum.Core
                    }
                },
                #endregion
                #region 78462c7e-4658-422d-9ae5-9e9c55b24276
                new DXRelationDefinitionUnit()
                {
                    ID = new Guid("78462c7e-4658-422d-9ae5-9e9c55b24276"),
                    DPRelationGenBlock = new DPRelationGenBlock()
                    {
                        ID = new Guid("ff669d1b-7635-4e23-b5f2-17563fe1e369"),
                        ObjectNameLeft = "DXRelationDefinitionUnit",
                        RelationNameLeft = "DXRelationDefinitionUnitID",
                        ObjectNameRight = "DPRelationGenBlock",
                        RelationNameRight = "DPRelationGenBlock",
                        RelationTable = null,
                        RelationType = DPRelationTypeEnum.ZeroOneToZeroOne,
                        Kind = DXObjectKindEnum.Core
                    }
                },
                #endregion
                #region 87f7a8f1-caea-4d67-b9d3-6c85aae00174
                new DXRelationDefinitionUnit()
                {
                    ID = new Guid("87f7a8f1-caea-4d67-b9d3-6c85aae00174"),
                    DPRelationGenBlock = new DPRelationGenBlock()
                    {
                        ID = new Guid("e0e600cc-00aa-4b96-982c-e2ccdd750403"),
                        ObjectNameLeft = "DPMigrationScriptsObject",
                        RelationNameLeft = "DPMigrationScriptsObjectID",
                        ObjectNameRight = "DPMigrationScriptsGenBlock",
                        RelationNameRight = "DPMigrationScriptsGenBlock",
                        RelationTable = null,
                        RelationType = DPRelationTypeEnum.ZeroOneToZeroOne,
                        Kind = DXObjectKindEnum.Core
                    }
                },
                #endregion
                #region 87964e3e-ed1b-42f5-a18e-9f7102ebb352
                new DXRelationDefinitionUnit()
                {
                    ID = new Guid("87964e3e-ed1b-42f5-a18e-9f7102ebb352"),
                    DPRelationGenBlock = new DPRelationGenBlock()
                    {
                        ID = new Guid("dfedf1fb-7573-4408-8618-2ff65a4ebf6c"),
                        ObjectNameLeft = "DXUnitDefinitionUnit",
                        RelationNameLeft = "DXUnitDefinitionUnitID",
                        ObjectNameRight = "DPEntityInheritanceBlock",
                        RelationNameRight = "DPEntityInheritanceBlock",
                        RelationTable = null,
                        RelationType = DPRelationTypeEnum.ZeroOneToMany,
                        Kind = DXObjectKindEnum.Core
                    }
                },
                #endregion
                #region d4901e1e-f9ff-431b-85cd-a48aa7dbf7fc
                new DXRelationDefinitionUnit()
                {
                    ID = new Guid("d4901e1e-f9ff-431b-85cd-a48aa7dbf7fc"),
                    DPRelationGenBlock = new DPRelationGenBlock()
                    {
                        ID = new Guid("f60dd08e-cf27-4592-8f84-4a8ba1fca8ff"),
                        ObjectNameLeft = "DXUnitDefinitionUnit",
                        RelationNameLeft = "DXUnitDefinitionUnitID",
                        ObjectNameRight = "DXElementInUnitDefinitionMainElement",
                        RelationNameRight = "DXElementInUnitDefinitionMainElement",
                        RelationTable = null,
                        RelationType = DPRelationTypeEnum.ZeroOneToMany,
                        Kind = DXObjectKindEnum.Core
                    }
                },
                #endregion
                #region e352d11e-1fb1-4433-8fe6-fe240bae963a
                new DXRelationDefinitionUnit()
                {
                    ID = new Guid("e352d11e-1fb1-4433-8fe6-fe240bae963a"),
                    DPRelationGenBlock = new DPRelationGenBlock()
                    {
                        ID = new Guid("d38cf673-89c7-47d7-9f48-5a3a49396ed2"),
                        ObjectNameLeft = "DXUnitDefinitionMainElement",
                        RelationNameLeft = "DXUnitDefinitionMainElement",
                        ObjectNameRight = "DXObjectKindEnum",
                        RelationNameRight = "Kind",
                        RelationTable = null,
                        RelationType = DPRelationTypeEnum.ManyToOne,
                        Kind = DXObjectKindEnum.Core,
                        RelationColumnNameRight = "Key",
                        RelationColumnTypeRight = DXColumnTypeEnum.Int
                    }
                },
                #endregion
                #region fb14d9b9-49a3-4ff5-8101-ac14c7d0ca91
                new DXRelationDefinitionUnit()
                {
                    ID = new Guid("fb14d9b9-49a3-4ff5-8101-ac14c7d0ca91"),
                    DPRelationGenBlock = new DPRelationGenBlock()
                    {
                        ID = new Guid("7b843027-7c06-40ac-9872-8572cadc66f2"),
                        ObjectNameLeft = "DXColumnDefinitionElement",
                        RelationNameLeft = "DXColumnDefinitionElement",
                        ObjectNameRight = "DXColumnTypeEnum",
                        RelationNameRight = "ColumnType",
                        RelationTable = null,
                        RelationType = DPRelationTypeEnum.ManyToOne,
                        Kind = DXObjectKindEnum.Core,
                        RelationColumnNameRight = "Key",
                        RelationColumnTypeRight = DXColumnTypeEnum.Int
                    }
                },
                #endregion
                #region ecd64009-c91e-4630-aff4-2948f5d0a3df
                new DXRelationDefinitionUnit()
                {
                    ID = new Guid("ecd64009-c91e-4630-aff4-2948f5d0a3df"),
                    DPRelationGenBlock = new DPRelationGenBlock()
                    {
                        ID = new Guid("572d875f-66cf-4850-8bc7-45805b93e093"),
                        ObjectNameLeft = "DXColumnDefinitionElement",
                        RelationNameLeft = "Enums",
                        ObjectNameRight = "DXColumnDefinitionElement",
                        RelationNameRight = "EnumKey",
                        RelationTable = null,
                        RelationType = DPRelationTypeEnum.ManyToZeroOne,
                        Kind = DXObjectKindEnum.Core
                    }
                },
                #endregion
                #region 298099bc-d243-40a0-8807-769a8f307809
                new DXRelationDefinitionUnit()
                {
                    ID = new Guid("298099bc-d243-40a0-8807-769a8f307809"),
                    DPRelationGenBlock = new DPRelationGenBlock()
                    {
                        ID = new Guid("5ccfd33e-f6b4-4374-bcbe-de3823b6ada4"),
                        ObjectNameLeft = "DXColumnDefinitionElement",
                        RelationNameLeft = "Enums",
                        ObjectNameRight = "DXEnumDefinitionUnit",
                        RelationNameRight = "EnumType",
                        RelationTable = null,
                        RelationType = DPRelationTypeEnum.ManyToZeroOne,
                        Kind = DXObjectKindEnum.Core
                    }
                },
                #endregion
                #region f665b056-ee22-482e-8da2-f55aa26a384c
                new DXRelationDefinitionUnit()
                {
                    ID = new Guid("f665b056-ee22-482e-8da2-f55aa26a384c"),
                    DPRelationGenBlock = new DPRelationGenBlock()
                    {
                        ID = new Guid("4aafad60-92df-41ee-b9be-a57125740b12"),
                        ObjectNameLeft = "DXElementInUnitDefinitionMainElement",
                        RelationNameLeft = "DXElementInUnitDefinitionMainElement",
                        ObjectNameRight = "DXElementInUnitTypeEnum",
                        RelationNameRight = "RelationType",
                        RelationTable = null,
                        RelationType = DPRelationTypeEnum.ManyToOne,
                        Kind = DXObjectKindEnum.Core,
                        RelationColumnNameRight = "Key",
                        RelationColumnTypeRight = DXColumnTypeEnum.Int
                    }
                },
                #endregion
                #region 4f58bcbb-8398-4481-8a8f-ee936c79431f
                new DXRelationDefinitionUnit()
                {
                    ID = new Guid("4f58bcbb-8398-4481-8a8f-ee936c79431f"),
                    DPRelationGenBlock = new DPRelationGenBlock()
                    {
                        ID = new Guid("fe83cc43-0d3a-412b-bd93-d337717967af"),
                        ObjectNameLeft = "DXElementInUnitDefinitionMainElement",
                        RelationNameLeft = "DXUnitDefinitionUnit",
                        ObjectNameRight = "DXElementDefinitionUnit",
                        RelationNameRight = "DXElementDefinitionUnit",
                        RelationTable = null,
                        RelationType = DPRelationTypeEnum.ManyToOne,
                        Kind = DXObjectKindEnum.Core
                    }
                },
                #endregion
                #region 1e1611cd-ff11-495c-82f6-5f0871cdc05c
                new DXRelationDefinitionUnit()
                {
                    ID = new Guid("1e1611cd-ff11-495c-82f6-5f0871cdc05c"),
                    DPRelationGenBlock = new DPRelationGenBlock()
                    {
                        ID = new Guid("ebde3bed-352c-40e7-ac75-c05066f8ca05"),
                        ObjectNameLeft = "DPEntityInheritanceBlock",
                        RelationNameLeft = "ChildEntities",
                        ObjectNameRight = "DXUnitDefinitionUnit",
                        RelationNameRight = "BaseEntity",
                        RelationTable = null,
                        RelationType = DPRelationTypeEnum.ManyToOne,
                        Kind = DXObjectKindEnum.Core
                    }
                },
                #endregion
                #region 1e1611cd-ff11-495c-82f6-5f0871cdc05c
                new DXRelationDefinitionUnit()
                {
                    ID = new Guid("1e1611cd-ff11-495c-82f6-5f0871cdc05c"),
                    DPRelationGenBlock = new DPRelationGenBlock()
                    {
                        ID = new Guid("ebde3bed-352c-40e7-ac75-c05066f8ca05"),
                        ObjectNameLeft = "DPRelationGenBlock",
                        RelationNameLeft = "DPRelationGenBlock",
                        ObjectNameRight = "DXObjectKindEnum",
                        RelationNameRight = "Kind",
                        RelationTable = null,
                        RelationType = DPRelationTypeEnum.ManyToOne,
                        Kind = DXObjectKindEnum.Core,
                        RelationColumnNameRight = "Key",
                        RelationColumnTypeRight = DXColumnTypeEnum.Int
                    }
                },
                #endregion
                #region 7328a3c1-0d27-474c-90be-c96c5967d54f
                new DXRelationDefinitionUnit()
                {
                    ID = new Guid("7328a3c1-0d27-474c-90be-c96c5967d54f"),
                    DPRelationGenBlock = new DPRelationGenBlock()
                    {
                        ID = new Guid("c5bc5c06-1d3c-4767-b13c-887623ecb2ae"),
                        ObjectNameLeft = "DPRelationGenBlock",
                        RelationNameLeft = "DPRelationGenBlock",
                        ObjectNameRight = "DPRelationTypeEnum",
                        RelationNameRight = "RelationType",
                        RelationTable = null,
                        RelationType = DPRelationTypeEnum.ManyToOne,
                        Kind = DXObjectKindEnum.Core,
                        RelationColumnNameRight = "Key",
                        RelationColumnTypeRight = DXColumnTypeEnum.Int
                    }
                },
                #endregion
            };
        }
    }
}
