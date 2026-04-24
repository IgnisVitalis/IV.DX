using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXDerivedUnit")]
    internal class DXDerivedUnit : DXUnit
    {
        public DXMultiElementsContainer<DXDerivedElement> DXDerivedElement { get; set; } = null!;
    }    
}