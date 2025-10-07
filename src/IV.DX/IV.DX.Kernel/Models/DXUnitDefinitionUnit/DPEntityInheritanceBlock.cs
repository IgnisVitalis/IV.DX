using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [ESQLBlockDefinition("DXUnitInheritanceElement")]
    public class DXUnitInheritanceElement : ESQLBlock
    {
        [ESQLColumnDefinition("BaseEntity")]
        public Guid BaseEntity { get; set; }
    }
}