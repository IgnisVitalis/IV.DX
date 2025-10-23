using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using System;
using System.Collections.Generic;

namespace IV.DX.Shared.IntTests.Factories
{
    public static class DXElementFactory
    {
        public static DXElementDefinitionUnit GetItem(Guid id)
        {
            return new DXElementDefinitionUnit()
            {
                ID = id,
                DXObjectDefinitionMainElement = new DXObjectDefinitionMainElement()
                {
                    ID = Guid.NewGuid(),
                    Name = "Name",
                    Kind = DXObjectKindEnum.Custom,
                    DXUnitID = id
                },
                DXColumnDefinitionElement = new DXMultiElementsContainer<DXColumnDefinitionElement>()
                {
                    Mode = MultiElementsMode.Full,
                    Announced = new HashSet<DXColumnDefinitionElement>()
                    {
                        new DXColumnDefinitionElement()
                        {
                            ID= Guid.NewGuid(),
                            DXUnitID = id,
                            ColumnType = DXColumnTypeEnum.GUID,
                            Name = "NameGUID"
                        },
                        new DXColumnDefinitionElement()
                        {
                            ID= Guid.NewGuid(),
                            DXUnitID = id,
                            ColumnType = DXColumnTypeEnum.String,
                            Name = "NameString"
                        }
                    }
                }
            };
        }
    }
}
