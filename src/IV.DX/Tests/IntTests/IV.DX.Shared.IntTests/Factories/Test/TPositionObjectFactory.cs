using IV.DataProvider.Persistence.Shared.IntTests.Models.Test;
using System;

namespace IV.DataProvider.Persistence.Shared.IntTests.Factories.Test
{
    public static class TPositionObjectFactory
    {
        public static TPositionObject GetItem(Guid id, string name)
        {
            return new TPositionObject()
            {
                ID = id,
                //User = user.ID,
                TPositionGenBlock = new TPositionGenBlock()
                {
                    ID = Guid.NewGuid(),
                    ObjectID = id,
                    Name = name
                }
            };
        }
    }
}