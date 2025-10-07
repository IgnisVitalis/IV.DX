using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using System;
using System.Collections.Generic;

namespace IV.DataProvider.Persistence.Shared.IntTests.Factories
{
    public static class BlockFactory
    {
        public static DXElementDefinitionUnit GetItem(Guid id)
        {
            return new DXElementDefinitionUnit()
            {
                ID = id,
                DXUnitDefinitionMainElement = new DXUnitDefinitionMainElement()
                {
                    ID = Guid.NewGuid(),
                    Name = "Name",
                    Kind = DPObjectKindEnum.Custom,
                    ObjectID = id
                },
                DXColumnDefinitionElement = new ESQLMultiItemsContainer<DXColumnDefinitionElement>()
                {
                    Mode = ModeForMultiItems.Full,
                    Announced = new List<DXColumnDefinitionElement>()
                    {
                        new DXColumnDefinitionElement()
                        {
                            ID= Guid.NewGuid(),
                            ObjectID = id,
                            ColumnType = DPColumnTypeEnum.GUID,
                            Name = "NameGUID"
                        },
                        new DXColumnDefinitionElement()
                        {
                            ID= Guid.NewGuid(),
                            ObjectID = id,
                            ColumnType = DPColumnTypeEnum.String,
                            Name = "NameString"
                        }
                    }
                }
            };
        }
    }
}
