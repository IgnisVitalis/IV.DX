using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXRelationDefinitionUnit")]
    public class DXRelationDefinitionUnit : DXUnit
    {
        [DXColumn("RelationType")]
        public DXRelationTypeEnum RelationType { get; set; }
        [DXColumn("ObjectNameLeft")]
        public string ObjectNameLeft { get; set; } = null!;
        [DXColumn("RelationNameLeft")]
        public string RelationNameLeft { get; set; } = null!;
        [DXColumn("ObjectNameRight")]
        public string ObjectNameRight { get; set; } = null!;
        [DXColumn("RelationNameRight")]
        public string RelationNameRight { get; set; } = null!;
        [DXColumn("RelationTable")]
        public string? RelationTable { get; set; }
        [DXColumn("Kind")]
        public DXObjectKindEnum Kind { get; set; }
        [DXColumn("RelationColumnNameLeft")]
        public string? RelationColumnNameLeft { get; set; }

        [DXColumn("RelationColumnTypeLeft")]
        public DXColumnTypeEnum? RelationColumnTypeLeft { get; set; }

        [DXColumn("RelationColumnNameRight")]
        public string? RelationColumnNameRight { get; set; }

        [DXColumn("RelationColumnTypeRight")]
        public DXColumnTypeEnum? RelationColumnTypeRight { get; set; }

        public DXRelationDefinitionUnit()
        {
            this.Kind = DXObjectKindEnum.Custom;
        }

        public DXRelationDefinitionUnit CreateInvertedRelationObject()
        {
            var objectId = Guid.NewGuid();

            this.Kind = DXObjectKindEnum.Custom;

            return new DXRelationDefinitionUnit()
            {
                Id = objectId,


                ObjectNameLeft = this.ObjectNameRight,
                ObjectNameRight = this.ObjectNameLeft,
                RelationNameLeft = this.RelationNameRight,
                RelationNameRight = this.RelationNameLeft,
                RelationTable = this.RelationTable,
                RelationType = DXRelationTypeEnumHelper.GetInvertedRelationType(this.RelationType),
                RelationColumnNameLeft = this.RelationColumnNameRight,
                RelationColumnNameRight = this.RelationColumnNameLeft,
                RelationColumnTypeLeft = this.RelationColumnTypeRight,
                RelationColumnTypeRight = this.RelationColumnTypeLeft,
                Kind = this.Kind

            };
        }

        public string GetQueryForInvertedRelationObject()
        {
            return $@"ObjectNameRight = '{this.ObjectNameLeft}' 
                    AND ObjectNameLeft = '{this.ObjectNameRight}'
                    AND RelationNameRight = '{this.RelationNameLeft}'
                    AND RelationNameLeft = '{this.RelationNameRight}'";
        }
    }
}