using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXGroupUnit")]
    public class DXGroupUnit : DXSecurityMemberUnit
    {
        [DXColumn("Tenant", "Tenant", DXLoadingType.Base)]
        public Guid Tenant { get; set; }
    }
}

