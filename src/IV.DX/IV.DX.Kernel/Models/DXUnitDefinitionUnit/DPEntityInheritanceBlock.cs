using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXElement("DXUnitInheritanceElement")]
    public class DXUnitInheritanceElement : DXElement
    {
        [DXColumn("BaseEntity")]
        public Guid BaseEntity { get; set; }
    }
}