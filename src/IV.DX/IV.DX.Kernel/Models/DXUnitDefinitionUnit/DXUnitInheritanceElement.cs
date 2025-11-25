using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;

namespace IV.DX.Kernel.Models
{
    [DXElement("DXUnitRelationElement")]
    public class DXUnitRelationElement : DXElement
    {
        [DXColumn("OwnRelationName")]
        public string OwnRelationName { get; set; }
        [DXColumn("TargetRelationName")]
        public string TargetRelationName { get; set; }
        [DXColumn("RelationType")]
        public DXRelationTypeEnum RelationType { get; set; }
        [DXColumn("TargetUnit")]
        public Guid TargetUnit { get; set; }
    }
}