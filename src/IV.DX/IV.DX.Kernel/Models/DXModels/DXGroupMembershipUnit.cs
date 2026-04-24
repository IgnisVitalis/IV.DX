using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXGroupMembershipUnit")]
    public class DXGroupMembershipUnit : DXUnit
    {
        [DXColumn("Group", "Group", DXLoadingType.Base)]
        public Guid Group { get; set; }

        [DXColumn("Membership", "Membership", DXLoadingType.Base)]
        public Guid Membership { get; set; }
    }
}

