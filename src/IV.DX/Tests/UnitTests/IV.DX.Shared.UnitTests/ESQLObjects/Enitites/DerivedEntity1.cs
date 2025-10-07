using IV.DataProvider.Persistence.Shared.UnitTests.ESQLObjects.Blocks;
using IV.DX.Kernel.Attributes;

namespace IV.DataProvider.Persistence.Shared.UnitTests.ESQLObjects.Enitites
{
    [ESQLObjectDefinition("Entity")]
    public class DerivedEntity1 : BaseEntity1
    {
        public DerivedBlock1 DerivedBlock1 { get; set; }
    }
}
