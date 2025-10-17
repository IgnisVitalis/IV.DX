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
                ID = id,
                User = user.ID,
                TPassportMainElement = new TPassportMainElement()
                {
                    ID = Guid.NewGuid(),
                    ObjectID = id,
                    SerialNumber = serialNumber
                }
            };
        }
    }
}