using IV.DataProvider.Persistence.Shared.IntTests.Models.Test;
using System;

namespace IV.DataProvider.Persistence.Shared.IntTests.Factories.Test
{
    public static class TDeviceObjectFactory
    {
        public static TDeviceObject GetItem(Guid id, string model, Guid uuid, TUserObject user)
        {
            return new TDeviceObject()
            {
                ID = id,
                User = user.ID,
                TDeviceGenBlock = new TDeviceGenBlock()
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