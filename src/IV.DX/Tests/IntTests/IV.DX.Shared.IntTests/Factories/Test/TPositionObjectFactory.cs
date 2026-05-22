using IV.DX.Shared.IntTests.Models.Test;
using System;

namespace IV.DX.Shared.IntTests.Factories.Test
{
    public static class TPositionUnitFactory
    {
        public static TPositionUnit GetItem(string name)
        {
            return new TPositionUnit()
            {
                TPositionMainElement = new TPositionMainElement()
                {
                    Id = Guid.NewGuid(),
                    Name = name
                }
            };
        }
    }
}