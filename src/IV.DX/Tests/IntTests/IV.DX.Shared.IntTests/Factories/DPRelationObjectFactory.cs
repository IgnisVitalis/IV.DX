using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using System;

namespace IV.DataProvider.Persistence.Shared.IntTests.Factories
{
    public static class DXRelationDefinitionUnitFactory
    {
        public static DXRelationDefinitionUnit GetItem(
            Guid id,
            DPRelationTypeEnum relationType,
            string relationNameLeft,
            string relationNameRight,
            DPObjectDescObject objectNameLeft,
            DPObjectDescObject objectNameRight,
            string realationTable = null)
        {
            DXRelationDefinitionUnit obj = new DXRelationDefinitionUnit()
            {
                ID = id,
                DXRelationDefinitionMainElement = new DXRelationDefinitionMainElement()
                {
                    ID = Guid.NewGuid(),
                    ObjectID = id,
                    RelationType = relationType,
                    RelationNameLeft = relationNameLeft,
                    RelationNameRight = relationNameRight,
                    ObjectNameLeft = objectNameLeft.DXUnitDefinitionMainElement.Name,
                    ObjectNameRight = objectNameRight.DXUnitDefinitionMainElement.Name,
                    RelationTable = realationTable
                }
            };

            return obj;
        }
    }
}