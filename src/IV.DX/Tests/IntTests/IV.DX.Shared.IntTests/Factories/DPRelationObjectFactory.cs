using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using System;

namespace IV.DataProvider.Persistence.Shared.IntTests.Factories
{
    public static class DPRelationObjectFactory
    {
        public static DPRelationObject GetItem(
            Guid id,
            DPRelationTypeEnum relationType,
            string relationNameLeft,
            string relationNameRight,
            DPObjectDescObject objectNameLeft,
            DPObjectDescObject objectNameRight,
            string realationTable = null)
        {
            DPRelationObject obj = new DPRelationObject()
            {
                ID = id,
                DPRelationGenBlock = new DPRelationGenBlock()
                {
                    ID = Guid.NewGuid(),
                    ObjectID = id,
                    RelationType = relationType,
                    RelationNameLeft = relationNameLeft,
                    RelationNameRight = relationNameRight,
                    ObjectNameLeft = objectNameLeft.DPObjectDescGenBlock.Name,
                    ObjectNameRight = objectNameRight.DPObjectDescGenBlock.Name,
                    RelationTable = realationTable
                }
            };

            return obj;
        }
    }
}