using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [ESQLBlockDefinition("DPEntityInheritanceBlock")]
    public class DPEntityInheritanceBlock : ESQLBlock
    {
        [ESQLColumnDefinition("BaseEntity")]
        public Guid BaseEntity { get; set; }
    }
}