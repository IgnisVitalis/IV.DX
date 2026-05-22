using IV.DX.Shared.IntTests.Models.Test;
using System;

namespace IV.DX.Shared.IntTests.Factories.Test
{
    public static class TDocumentUnitFactory
    {
        public static TDocumentUnit GetItem(string name)
        {
            return new TDocumentUnit()
            {
                //User = user.Id,
                TDocumentMainElement = new TDocumentMainElement()
                {
                    Id = Guid.NewGuid(),
                    Name = name
                }
            };
        }
    }
}