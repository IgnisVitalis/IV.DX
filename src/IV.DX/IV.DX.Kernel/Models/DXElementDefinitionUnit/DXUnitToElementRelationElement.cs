using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;

namespace IV.DX.Kernel.Models
{
    [DXElement("DXElementToUnitRelationElement")]
    public class DXElementToUnitRelationElement : DXElement
    {
        [DXColumn("OwnRelationName")]
        public string OwnRelationName { get; set; } = null!;
        [DXColumn("TargetRelationName")]
        public string TargetRelationName { get; set; } = null!;
        [DXColumn("RelationType")]
        public DXRelationTypeEnum RelationType { get; set; }
        [DXColumn("TargetDXUnit")]
        public Guid TargetDXUnit { get; set; }

        public DXUnitToElementRelationElement GetReverted()
        {
            return new DXUnitToElementRelationElement()
            {
                OwnRelationName = this.TargetRelationName,
                TargetRelationName = this.OwnRelationName,
                TargetDXElement = this.DXUnitID,
                RelationType = DXRelationTypeEnumHelper.GetInvertedRelationType(this.RelationType)
            };
        }
    }
}
