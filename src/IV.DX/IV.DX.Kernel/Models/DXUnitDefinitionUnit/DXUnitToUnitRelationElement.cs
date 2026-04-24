using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;

namespace IV.DX.Kernel.Models
{
    [DXElement("DXUnitToUnitRelationElement")]
    public class DXUnitToUnitRelationElement : DXElement
    {
        [DXColumn("OwnRelationName")]
        public string OwnRelationName { get; set; } = null!;
        [DXColumn("TargetRelationName")]
        public string TargetRelationName { get; set; } = null!;
        [DXColumn("RelationType")]
        public DXRelationTypeEnum RelationType { get; set; }
        [DXColumn("TargetDXUnit")]
        public Guid TargetDXUnit { get; set; }

        public DXUnitToUnitRelationElement GetReverted()
        {
            return new DXUnitToUnitRelationElement()
            {
                OwnRelationName = this.TargetRelationName,
                TargetRelationName = this.OwnRelationName,
                TargetDXUnit = this.DXUnitID,
                RelationType = DXRelationTypeEnumHelper.GetInvertedRelationType(this.RelationType)
            };
        }
    }
}