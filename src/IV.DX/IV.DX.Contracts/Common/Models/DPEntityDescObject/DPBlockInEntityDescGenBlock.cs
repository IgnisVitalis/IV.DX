using IV.DataProvider.Persistence.Contracts.Models;
using IV.DX.Contracts.Common.Attributes;
using IV.DX.Contracts.Common.Enums;

namespace IV.DX.Contracts.Common.Models
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