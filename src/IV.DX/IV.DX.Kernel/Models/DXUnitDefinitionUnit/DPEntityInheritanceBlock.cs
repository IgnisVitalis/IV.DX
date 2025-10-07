using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXElement("DXUnitInheritanceElement")]
    public class DXUnitInheritanceElement : ESQLBlock
    {
        [DXColumn("BaseEntity")]
        public Guid BaseEntity { get; set; }
    }
}