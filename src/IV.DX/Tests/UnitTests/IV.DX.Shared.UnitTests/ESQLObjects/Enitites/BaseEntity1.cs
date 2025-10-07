using IV.DataProvider.Persistence.Contracts.Models;
using IV.DataProvider.Persistence.Shared.UnitTests.ESQLObjects.Blocks;

namespace IV.DataProvider.Persistence.Shared.UnitTests.ESQLObjects.Enitites
{
    public abstract class BaseEntity1 : ESQLObject
    {
        public BaseBlock1 BaseBlock1 { get; set; }
    }
}
