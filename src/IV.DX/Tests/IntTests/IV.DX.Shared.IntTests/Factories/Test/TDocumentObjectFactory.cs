using IV.DataProvider.Persistence.Shared.IntTests.Models.Test;
using System;

namespace IV.DataProvider.Persistence.Shared.IntTests.Factories.Test
{
    public static class TDocumentObjectFactory
    {
        public static TDocumentObject GetItem(Guid id, string name)
        {
            return new TDocumentObject()
            {
                ID = id,
                //User = user.ID,
                TDocumentGenBlock = new TDocumentGenBlock()
                {
                    ID = Guid.NewGuid(),
                    ObjectID = id,
                    Name = name
                }
            };
        }
    }
}