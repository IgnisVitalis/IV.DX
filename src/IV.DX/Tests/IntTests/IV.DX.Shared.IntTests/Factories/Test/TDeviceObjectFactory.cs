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
                Id = id,
                User = user.Id,
                TDeviceMainElement = new TDeviceMainElement()
                {
                    Id = Guid.NewGuid(),
                    DXUnitId = id,
                    Model = model,
                    UUID = uuid
                }
            };
        }
    }
}