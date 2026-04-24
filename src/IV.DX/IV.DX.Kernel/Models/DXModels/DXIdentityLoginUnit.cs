using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXIdentityLoginUnit")]
    public class DXIdentityLoginUnit : DXUnit
    {
        [DXColumn("Subject")]
        public string Subject { get; set; } = null!;

        [DXColumn("SecretHash")]
        public string? SecretHash { get; set; }

        [DXColumn("Provider")]
        public DXIdentityProviderTypeEnum Provider { get; set; }

        [DXColumn("Identity", "Identity", DXLoadingType.Base)]
        public Guid Identity { get; set; }
    }
}
