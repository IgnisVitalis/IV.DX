using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXIdentityOwnershipUnit")]
    public class DXIdentityOwnershipUnit : DXUnit
    {
        [DXColumn("Identity")]
        public Guid Identity { get; set; }

        [DXColumn("DXUnitDefinition")]
        public Guid DXUnitDefinition { get; set; }

        [DXColumn("OwnedDXUnitID")]
        public Guid OwnedDXUnitID { get; set; }
    }
}