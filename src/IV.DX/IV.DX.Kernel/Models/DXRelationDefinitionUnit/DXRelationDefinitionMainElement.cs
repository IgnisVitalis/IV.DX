using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;

namespace IV.DX.Kernel.Models
{
    [DXElement("DXRelationDefinitionMainElement")]
    public class DXRelationDefinitionMainElement : DXElement
    {
        [DXColumn("RelationType")]
        public DXRelationTypeEnum RelationType { get; set; }
        [DXColumn("ObjectNameLeft")]
        public string ObjectNameLeft { get; set; }
        [DXColumn("RelationNameLeft")]
        public string RelationNameLeft { get; set; }
        [DXColumn("ObjectNameRight")]
        public string ObjectNameRight { get; set; }
        [DXColumn("RelationNameRight")]
        public string RelationNameRight { get; set; }
        [DXColumn("RelationTable")]
        public string RelationTable { get; set; }
        [DXColumn("Kind")]
        public DXObjectKindEnum Kind { get; set; }
        [DXColumn("RelationColumnNameLeft")]
        public string RelationColumnNameLeft { get; set; }

        [DXColumn("RelationColumnTypeLeft")]
        public DXColumnTypeEnum? RelationColumnTypeLeft { get; set; }

        [DXColumn("RelationColumnNameRight")]
        public string RelationColumnNameRight { get; set; }

        [DXColumn("RelationColumnTypeRight")]
        public DXColumnTypeEnum? RelationColumnTypeRight { get; set; }

        public DXRelationDefinitionMainElement()
        {
            this.Kind = DXObjectKindEnum.Custom;
        }
    }
}