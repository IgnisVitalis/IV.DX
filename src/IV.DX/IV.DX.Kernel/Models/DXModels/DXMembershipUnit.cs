using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXMembershipUnit")]
    public class DXMembershipUnit : DXSecurityMemberUnit
    {
        [DXColumn("Identity", "Identity", DXLoadingType.Base)]
        public Guid Identity { get; set; }

        [DXColumn("Tenant", "Tenant", DXLoadingType.Base)]
        public Guid Tenant { get; set; }
    }
}
