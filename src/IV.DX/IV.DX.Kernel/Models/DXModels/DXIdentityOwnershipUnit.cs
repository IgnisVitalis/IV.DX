using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    /// <summary>
    /// Instance-level grant tying an identity to a single record. The operation flags say what
    /// the owner may do with it, so co-owners can hold different rights over the same record.
    /// </summary>
    [DXUnit("DXIdentityOwnershipUnit")]
    public class DXIdentityOwnershipUnit : DXUnit
    {
        [DXColumn("Identity")]
        public Guid Identity { get; set; }

        [DXColumn("DXUnitDefinition")]
        public Guid DXUnitDefinition { get; set; }

        [DXColumn("OwnedDXUnitId")]
        public Guid OwnedDXUnitId { get; set; }

        [DXColumn("Read")]
        public bool Read { get; set; }

        [DXColumn("Update")]
        public bool Update { get; set; }

        [DXColumn("Delete")]
        public bool Delete { get; set; }

        /// <summary>Deny outranks any Allow on the same record, matching how role grants resolve.</summary>
        [DXColumn("Effect")]
        public DXGrantEffectEnum Effect { get; set; }
    }
}
