using IV.DataProvider.Persistence.Shared.IntTests.Models.Test;
using IV.DX.Kernel.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IV.DataProvider.Persistence.Shared.IntTests.Factories.Test
{
    public static class TBookObjectFactory
    {
        public static TBookObject GetItem(Guid id, string name)
        {
            return new TBookObject()
            {
                ID = id,
                TBookGenBlock = new TBookGenBlock()
                {
                    ID = Guid.NewGuid(),
                    ObjectID = id,
                    Name = name
                }
            };
        }

        public static TBookObject GetItemWithText(Guid id, string name, IEnumerable<string> text)
        {
            var result = GetItem(id, name);

            int number = 1;

            result.TBookChapterBlock = new DXMultiElementsContainer<TBookChapterBlock>()
            {
                Announced = text.Select(x =>
                    new TBookChapterBlock()
                    {
                        ID = Guid.NewGuid(),
                        ObjectID = result.ID,
                        Text = x,
                        Number = number++
                    })
            };

            return result;
        }
    }
}