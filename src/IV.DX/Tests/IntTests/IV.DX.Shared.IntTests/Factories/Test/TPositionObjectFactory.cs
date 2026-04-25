using IV.DX.Shared.IntTests.Models.Test;
using System;

namespace IV.DX.Shared.IntTests.Factories.Test
{
    public static class TPositionUnitFactory
    {
        public static TPositionUnit GetItem(Guid id, string name)
        {
            return new TPositionUnit()
            {
                Id = id,
                //User = user.Id,
                TPositionMainElement = new TPositionMainElement()
                {
                    Id = Guid.NewGuid(),
                    DXUnitId = id,
                    Name = name
                }
            };
        }
    }
}