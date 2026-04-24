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
                ID = id,
                //User = user.ID,
                TPositionMainElement = new TPositionMainElement()
                {
                    ID = Guid.NewGuid(),
                    DXUnitID = id,
                    Name = name
                }
            };
        }
    }
}