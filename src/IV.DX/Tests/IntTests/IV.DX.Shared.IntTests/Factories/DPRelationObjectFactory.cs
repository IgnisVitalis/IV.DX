using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using System;

namespace IV.DX.Shared.IntTests.Factories
{
    public static class DXRelationDefinitionUnitFactory
    {
        public static DXRelationDefinitionUnit GetItem(
            Guid id,
            DXRelationTypeEnum relationType,
            string relationNameLeft,
            string relationNameRight,
            DXObjectDefinitionUnit objectNameLeft,
            DXObjectDefinitionUnit objectNameRight,
            string realationTable = null)
        {
            string relationColumnNameLeft = null;
            string relationColumnNameRight = null;
            DXColumnTypeEnum? relationColumnTypeLeft = null;
            DXColumnTypeEnum? relationColumnTypeRight = null;
            string relationTable = null;

            switch (relationType)
            {
                case DXRelationTypeEnum.ManyToMany:
                    relationTable = $"Relation_{objectNameLeft.Name}_{objectNameRight.Name}";
                    relationColumnNameLeft = "Id";
                    relationColumnTypeLeft = DXColumnTypeEnum.GUID;
                    relationColumnNameRight = "Id";
                    relationColumnTypeRight = DXColumnTypeEnum.GUID;
                    break;
                case DXRelationTypeEnum.ManyToOne:
                case DXRelationTypeEnum.ManyToZeroOne:
                case DXRelationTypeEnum.ZeroOneToOne:
                    {
                        relationColumnNameLeft = relationNameRight;
                        relationColumnTypeLeft = DXColumnTypeEnum.GUID;
                        relationColumnNameRight = "Id";
                        relationColumnTypeRight = DXColumnTypeEnum.GUID;
                    }
                    break;
                case DXRelationTypeEnum.OneToMany:
                case DXRelationTypeEnum.ZeroOneToMany:
                case DXRelationTypeEnum.OneToZeroOne:
                case DXRelationTypeEnum.ZeroOneToZeroOne:
                    {
                        relationColumnNameLeft = "Id";
                        relationColumnTypeLeft = DXColumnTypeEnum.GUID;
                        relationColumnNameRight = relationNameLeft;
                        relationColumnTypeRight = DXColumnTypeEnum.GUID;
                    }
                    break;
            }

            DXRelationDefinitionUnit obj = new DXRelationDefinitionUnit()
            {
                Id = id,
                RelationType = relationType,
                RelationNameLeft = relationNameLeft,
                RelationNameRight = relationNameRight,
                ObjectNameLeft = objectNameLeft.Name,
                ObjectNameRight = objectNameRight.Name,
                RelationTable = realationTable,
                RelationColumnNameLeft = relationColumnNameLeft,
                RelationColumnNameRight = relationColumnNameRight,
                RelationColumnTypeLeft = relationColumnTypeLeft,
                RelationColumnTypeRight = relationColumnTypeRight
            };

            return obj;
        }
    }
}