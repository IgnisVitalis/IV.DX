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
            DXRelationDefinitionUnit obj = new DXRelationDefinitionUnit()
            {
                ID = id,
                DXRelationDefinitionMainElement = new DXRelationDefinitionMainElement()
                {
                    ID = Guid.NewGuid(),
                    DXUnitID = id,
                    RelationType = relationType,
                    RelationNameLeft = relationNameLeft,
                    RelationNameRight = relationNameRight,
                    ObjectNameLeft = objectNameLeft.DXObjectDefinitionMainElement.Name,
                    ObjectNameRight = objectNameRight.DXObjectDefinitionMainElement.Name,
                    RelationTable = realationTable
                }
            };

            return obj;
        }
    }
}