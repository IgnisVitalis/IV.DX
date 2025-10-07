using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using System;

namespace IV.DataProvider.Persistence.Shared.IntTests.Factories
{
    public static class ObjectFactory
    {
        public static DXUnitDefinitionUnit GetItem(Guid id, string objectName)
        {
            DXUnitDefinitionUnit item = new DXUnitDefinitionUnit()
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