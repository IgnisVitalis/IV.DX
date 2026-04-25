using IV.DX.Shared.IntTests.Models.Test;
using System;

namespace IV.DX.Shared.IntTests.Factories.Test
{
    public static class TDocumentUnitFactory
    {
        public static TDocumentUnit GetItem(Guid id, string name)
        {
            return new TDocumentUnit()
            {
                Id = id,
                //User = user.Id,
                TDocumentMainElement = new TDocumentMainElement()
                {
                    Id = Guid.NewGuid(),
                    DXUnitId = id,
                    Name = name
                }
            };
        }
    }
}