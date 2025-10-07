using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using System;
using System.Collections.Generic;

namespace IV.DataProvider.Persistence.Shared.IntTests.Factories
{
    public static class BlockFactory
    {
        public static DPBlockDescObject GetItem(Guid id)
        {
            return new DPBlockDescObject()
            {
                ID = id,
                DPObjectDescGenBlock = new DPObjectDescGenBlock()
                {
                    ID = Guid.NewGuid(),
                    Name = "Name",
                    Kind = DPObjectKindEnum.Custom,
                    ObjectID = id
                },
                DPColumnDescBlock = new ESQLMultiItemsContainer<DPColumnDescBlock>()
                {
                    Mode = ModeForMultiItems.Full,
                    Announced = new List<DPColumnDescBlock>()
                    {
                        new DPColumnDescBlock()
                        {
                            ID= Guid.NewGuid(),
                            ObjectID = id,
                            ColumnType = DPColumnTypeEnum.GUID,
                            Name = "NameGUID"
                        },
                        new DPColumnDescBlock()
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
