using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXElement("DXDerivedElement")]
    internal class DXDerivedElement : DXElement
    {
        [DXColumn("DerivedDXUnitType")]
        public Guid DerivedDXUnitType { get; set; }
        [DXColumn("DXObjectID")]
        public Guid DXObjectID { get; set; }
    }
}
