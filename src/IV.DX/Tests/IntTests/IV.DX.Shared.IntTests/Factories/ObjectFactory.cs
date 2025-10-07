using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using System;

namespace IV.DX.Shared.IntTests.Factories
{
    public static class ObjectFactory
    {
        public static DXUnitDefinitionUnit GetItem(Guid id, string objectName)
        {
            DXUnitDefinitionUnit item = new DXUnitDefinitionUnit()
            {
                ID = id,
                DXUnitDefinitionMainElement = new DXUnitDefinitionMainElement()
                {
                    ID = Guid.NewGuid(),
                    ObjectID = id,
                    Name = objectName,
                    Kind = DXObjectKindEnum.Custom,
                }
            };

            return item;
        }
    }
}