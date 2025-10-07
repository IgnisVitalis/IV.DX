using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;

namespace IV.DX.Kernel.Models
{
    [ESQLBlockDefinition("DPBlockInEntityDescGenBlock")]
    public class DPBlockInEntityDescGenBlock : ESQLBlock
    {
        [ESQLColumnDefinition("RelationType")]
        public DPBlockInObjectTypeEnum RelationType { get; set; }

        [ESQLColumnDefinition("DPBlockDescObject")]
        public Guid DPBlockDescObject { get; set; }
    }
}