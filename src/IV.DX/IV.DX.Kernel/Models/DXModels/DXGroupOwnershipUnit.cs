using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    /// <summary>
    /// Instance-level grant tying a group to a single record. Every member of the group holds
    /// the operations flagged here, resolved through <c>DXExecutionContext.ActiveGroupIDs</c>.
    /// </summary>
    [DXUnit("DXGroupOwnershipUnit")]
    public class DXGroupOwnershipUnit : DXUnit
    {
        [DXColumn("Group")]
        public Guid Group { get; set; }

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
