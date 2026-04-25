using IV.DX.Kernel.Models;
using IV.DX.Shared.IntTests.Models.Test;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IV.DX.Shared.IntTests.Factories.Test
{
    public static class TBookUnitFactory
    {
        public static TBookUnit GetItem(Guid id, string name)
        {
            return new TBookUnit()
            {
                Id = id,
                TBookMainElement = new TBookMainElement()
                {
                    Id = Guid.NewGuid(),
                    DXUnitId = id,
                    Name = name
                }
            };
        }

        public static TBookUnit GetItemWithText(Guid id, string name, IEnumerable<string> text)
        {
            var result = GetItem(id, name);

            int number = 1;

            result.TBookChapterElement = new DXMultiElementsContainer<TBookChapterElement>()
            {
                Announced = text.Select(x =>
                    new TBookChapterElement()
                    {
                        Id = Guid.NewGuid(),
                        DXUnitId = result.Id,
                        Text = x,
                        Number = number++
                    }).ToHashSet()
            };

            return result;
        }
    }
}
