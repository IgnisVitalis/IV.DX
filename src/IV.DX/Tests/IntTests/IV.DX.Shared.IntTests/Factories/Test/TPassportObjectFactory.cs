using IV.DX.Shared.IntTests.Models.Test;
using System;

namespace IV.DX.Shared.IntTests.Factories.Test
{
    public static class TPassportUnitFactory
    {
        public static TPassportUnit GetItem(Guid id, string serialNumber, TUserUnit user)
        {
            return new TPassportUnit()
            {
                Id = id,
                User = user.Id,
                TPassportMainElement = new TPassportMainElement()
                {
                    Id = Guid.NewGuid(),
                    DXUnitId = id,
                    SerialNumber = serialNumber
                }
            };
        }
    }
}