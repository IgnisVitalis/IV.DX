using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;

namespace IV.DX.Kernel.Models
{
    [DXElement("DXUnitToElementRelationElement")]
    public class DXUnitToElementRelationElement : DXElement
    {
        [DXColumn("OwnRelationName")]
        public string OwnRelationName { get; set; } = null!;
        [DXColumn("TargetRelationName")]
        public string TargetRelationName { get; set; } = null!;
        [DXColumn("RelationType")]
        public DXRelationTypeEnum RelationType { get; set; }
        [DXColumn("TargetDXElement")]
        public Guid TargetDXElement { get; set; }

        public DXElementToUnitRelationElement GetReverted()
        {
            return new DXElementToUnitRelationElement()
            {
                OwnRelationName = this.TargetRelationName,
                TargetRelationName = this.OwnRelationName,
                TargetDXUnit = this.DXUnitId,
                RelationType = DXRelationTypeEnumHelper.GetInvertedRelationType(this.RelationType)
            };
        }
    }
}