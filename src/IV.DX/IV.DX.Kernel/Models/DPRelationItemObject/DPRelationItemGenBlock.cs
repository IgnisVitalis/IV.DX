using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXElement("DPRelationItemGenBlock")]
    public class DPRelationItemGenBlock : ESQLBlock
    {
        [DXColumn("ObjectTypeNameLeft")]
        public string ObjectTypeNameLeft { get; set; }
        [DXColumn("ObjectIDLeft")]
        public Guid ObjectIDLeft { get; set; }
        [DXColumn("RelationNameRight")]
        public string RelationNameRight { get; set; }
        [DXColumn("ObjectTypeNameRight")]
        public string ObjectTypeNameRight { get; set; }
        [DXColumn("ObjectIDRight")]
        public Guid ObjectIDRight { get; set; }
    }
}