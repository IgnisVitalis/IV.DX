using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXRelationItemUnit")]
    public class DXRelationItemUnit : DXUnit
    {
        [DXColumn("ObjectTypeNameLeft")]
        public string ObjectTypeNameLeft { get; set; } = null!;
        [DXColumn("DXUnitIDLeft")]
        public Guid DXUnitIDLeft { get; set; }
        [DXColumn("RelationNameRight")]
        public string RelationNameRight { get; set; } = null!;
        [DXColumn("ObjectTypeNameRight")]
        public string ObjectTypeNameRight { get; set; } = null!;
        [DXColumn("DXUnitIDRight")]
        public Guid DXUnitIDRight { get; set; }
    }
}