using IV.DataProvider.Persistence.Shared.IntTests.Models.Test;
using System;

namespace IV.DataProvider.Persistence.Shared.IntTests.Factories.Test
{
    public static class TUserObjectFactory
    {
        public static TUserObject GetItem(Guid id, string name, string surname, DateTime birth)
        {
            return new TUserObject()
            {
                ID = id,
                TUserGenBlock = new TUserGenBlock()
                {
                    ID = Guid.NewGuid(),
                    ObjectID = id,
                    Name = name,
                    Surname = surname,
                    Birth = birth
                }
            };
        }
    }
}