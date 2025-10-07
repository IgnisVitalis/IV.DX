using IV.DX.Contracts.Common.Enums;
using IV.DX.Contracts.Common.Models;
using System;

namespace IV.DataProvider.Persistence.Shared.IntTests.Factories
{
    public static class ObjectFactory
    {
        public static DPEntityDescObject GetItem(Guid id, string objectName)
        {
            DPEntityDescObject item = new DPEntityDescObject()
            {
                ID = id,
                DPObjectDescGenBlock = new DPObjectDescGenBlock()
                {
                    ID = Guid.NewGuid(),
                    ObjectID = id,
                    Name = objectName,
                    Kind = DPObjectKindEnum.Custom,
                }
            };

            return item;
        }
    }
}