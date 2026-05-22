using IV.DX.Shared.IntTests.Models.Test;
using System;

namespace IV.DX.Shared.IntTests.Factories.Test
{
    public static class TUserUnitFactory
    {
        public static TUserUnit GetItem(string name, string surname, DateTime birth)
        {
            return new TUserUnit()
            {
                TUserMainElement = new TUserMainElement()
                {
                    Name = name,
                    Surname = surname,
                    Birth = birth
                }
            };
        }
    }
}