using IV.DX.Kernel.Attributes;
using IV.DX.Shared.UnitTests.DXObjects.DXElements;

namespace IV.DX.Shared.UnitTests.DXObjects.DXUnits
{
    [DXUnit("Entity")]
    public class DerivedUnit1 : BaseUnit1
    {
        public DerivedElement1 DerivedBlock1 { get; set; }
    }
}
