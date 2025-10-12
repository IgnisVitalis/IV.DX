using IV.DX.Kernel.Attributes;
using IV.DX.Shared.UnitTests.DXObjects.DXElements;

namespace IV.DX.Shared.UnitTests.DXObjects.DXUnits
{
    [DXUnit("DXUnit")]
    public class DerivedUnit1 : BaseUnit1
    {
        public DerivedElement1 DerivedDXElement1 { get; set; }
    }
}
