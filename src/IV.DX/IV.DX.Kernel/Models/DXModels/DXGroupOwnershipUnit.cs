using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXGroupOwnershipUnit")]
    public class DXGroupOwnershipUnit : DXUnit
    {
        [DXColumn("Group")]
        public Guid Group { get; set; }

        [DXColumn("DXUnitDefinition")]
        public Guid DXUnitDefinition { get; set; }

        [DXColumn("OwnedDXUnitId")]
        public Guid OwnedDXUnitId { get; set; }
    }
}