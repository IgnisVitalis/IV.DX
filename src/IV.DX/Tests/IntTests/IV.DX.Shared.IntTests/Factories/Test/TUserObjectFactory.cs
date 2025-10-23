using IV.DX.Shared.IntTests.Models.Test;
using System;

namespace IV.DX.Shared.IntTests.Factories.Test
{
    public static class TUserUnitFactory
    {
        public static TUserUnit GetItem(Guid id, string name, string surname, DateTime birth)
        {
            return new TUserUnit()
            {
                ID = id,
                TUserMainElement = new TUserMainElement()
                {
                    ID = Guid.NewGuid(),
                    DXUnitID = id,
                    Name = name,
                    Surname = surname,
                    Birth = birth
                }
            };
        }
    }
}