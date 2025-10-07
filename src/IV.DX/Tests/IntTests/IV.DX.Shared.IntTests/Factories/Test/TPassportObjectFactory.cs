using IV.DataProvider.Persistence.Shared.IntTests.Models.Test;
using System;

namespace IV.DataProvider.Persistence.Shared.IntTests.Factories.Test
{
    public static class TPassportObjectFactory
    {
        public static TPassportObject GetItem(Guid id, string serialNumber, TUserObject user)
        {
            return new TPassportObject()
            {
                ID = id,
                User = user.ID,
                TPassportGenBlock = new TPassportGenBlock()
                {
                    ID = Guid.NewGuid(),
                    ObjectID = id,
                    SerialNumber = serialNumber
                }
            };
        }
    }
}