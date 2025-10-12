using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXElement("DXUnitInheritanceElement")]
    public class DXUnitInheritanceElement : DXElement
    {
        [DXColumn("BaseDXUnit")]
        public Guid BaseDXUnit { get; set; }
    }
}