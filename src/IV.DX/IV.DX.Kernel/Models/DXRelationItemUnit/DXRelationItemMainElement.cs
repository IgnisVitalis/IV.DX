using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXElement("DXRelationItemMainElement")]
    public class DXRelationItemMainElement : DXElement
    {
        [DXColumn("ObjectTypeNameLeft")]
        public string ObjectTypeNameLeft { get; set; }
        [DXColumn("DXUnitIDLeft")]
        public Guid DXUnitIDLeft { get; set; }
        [DXColumn("RelationNameRight")]
        public string RelationNameRight { get; set; }
        [DXColumn("ObjectTypeNameRight")]
        public string ObjectTypeNameRight { get; set; }
        [DXColumn("DXUnitIDRight")]
        public Guid DXUnitIDRight { get; set; }
    }
}