using IV.DX.Shared.IntTests.Models.Test;
using System;

namespace IV.DX.Shared.IntTests.Factories.Test
{
    public static class TDeviceUnitFactory
    {
        public static TDeviceUnit GetItem(string model, Guid uuid, TUserUnit user)
        {
            return new TDeviceUnit()
            {
                User = user.Id,
                TDeviceMainElement = new TDeviceMainElement()
                {
                    Model = model,
                    UUID = uuid
                }
            };
        }
    }
}