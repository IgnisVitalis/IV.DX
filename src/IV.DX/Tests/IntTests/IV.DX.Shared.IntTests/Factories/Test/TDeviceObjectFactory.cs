using IV.DX.Shared.IntTests.Models.Test;
using System;

namespace IV.DX.Shared.IntTests.Factories.Test
{
    public static class TDeviceUnitFactory
    {
        public static TDeviceUnit GetItem(Guid id, string model, Guid uuid, TUserUnit user)
        {
            return new TDeviceUnit()
            {
                ID = id,
                User = user.ID,
                TDeviceMainElement = new TDeviceMainElement()
                {
                    ID = Guid.NewGuid(),
                    ObjectID = id,
                    Model = model,
                    UUID = uuid
                }
            };
        }
    }
}