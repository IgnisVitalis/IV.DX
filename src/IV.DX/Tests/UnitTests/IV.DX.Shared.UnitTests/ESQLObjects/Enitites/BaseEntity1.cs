using IV.DataProvider.Persistence.Shared.UnitTests.ESQLObjects.Blocks;
using IV.DX.Kernel.Models;

namespace IV.DataProvider.Persistence.Shared.UnitTests.ESQLObjects.Enitites
{
    public abstract class BaseEntity1 : ESQLObject
    {
        public BaseBlock1 BaseBlock1 { get; set; }
    }
}
