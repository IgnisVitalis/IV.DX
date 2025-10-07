using IV.DataProvider.Persistence.Shared.IntTests.Models.Test;
using System;

namespace IV.DataProvider.Persistence.Shared.IntTests.Factories.Test
{
    public static class TDocumentUnitFactory
    {
        public static TDocumentUnit GetItem(Guid id, string name)
        {
            return new TDocumentUnit()
            {
                ID = id,
                //User = user.ID,
                TDocumentMainElement = new TDocumentMainElement()
                {
                    ID = Guid.NewGuid(),
                    ObjectID = id,
                    Name = name
                }
            };
        }
    }
}