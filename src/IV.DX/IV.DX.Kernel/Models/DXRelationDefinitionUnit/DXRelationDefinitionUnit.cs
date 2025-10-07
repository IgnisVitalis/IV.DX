using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters;
using IV.DX.Kernel.Enums;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXRelationDefinitionUnit")]
    public class DXRelationDefinitionUnit : DXUnit
    {
        public static DXModelDefinition ESQLModelDefinition { get; } = DXModelConverter.GetESQLModelDefinition<DXRelationDefinitionUnit>();

        public DXRelationDefinitionMainElement DXRelationDefinitionMainElement { get; set; }

        public DXRelationDefinitionUnit CreateInvertedRelationObject()
        {
            var objectId = Guid.NewGuid();

            return new DXRelationDefinitionUnit()
            {
                ID = objectId,
                DXRelationDefinitionMainElement = new DXRelationDefinitionMainElement()
                {
                    ID = Guid.NewGuid(),
                    ObjectID = objectId,
                    ObjectNameLeft = this.DXRelationDefinitionMainElement.ObjectNameRight,
                    ObjectNameRight = this.DXRelationDefinitionMainElement.ObjectNameLeft,
                    RelationNameLeft = this.DXRelationDefinitionMainElement.RelationNameRight,
                    RelationNameRight = this.DXRelationDefinitionMainElement.RelationNameLeft,
                    RelationTable = this.DXRelationDefinitionMainElement.RelationTable,
                    RelationType = this.GetInvertedRelationType(this.DXRelationDefinitionMainElement.RelationType),
                    RelationColumnNameLeft = this.DXRelationDefinitionMainElement.RelationColumnNameRight,
                    RelationColumnNameRight = this.DXRelationDefinitionMainElement.RelationColumnNameLeft,
                    RelationColumnTypeLeft = this.DXRelationDefinitionMainElement.RelationColumnTypeRight,
                    RelationColumnTypeRight = this.DXRelationDefinitionMainElement.RelationColumnTypeLeft
                }
            };
        }

        public string GetQueryForInvertedRelationObject()
        {
            return $@"DXRelationDefinitionMainElement.ObjectNameRight = '{this.DXRelationDefinitionMainElement.ObjectNameLeft}' 
                    AND DXRelationDefinitionMainElement.ObjectNameLeft = '{this.DXRelationDefinitionMainElement.ObjectNameRight}'
                    AND DXRelationDefinitionMainElement.RelationNameRight = '{this.DXRelationDefinitionMainElement.RelationNameLeft}'
                    AND DXRelationDefinitionMainElement.RelationNameLeft = '{this.DXRelationDefinitionMainElement.RelationNameRight}'";
        }

        private DXRelationTypeEnum GetInvertedRelationType(DXRelationTypeEnum value)
        {
            switch (value)
            {
                case DXRelationTypeEnum.OneToZeroOne:
                    return DXRelationTypeEnum.ZeroOneToOne;
                case DXRelationTypeEnum.ZeroOneToOne:
                    return DXRelationTypeEnum.OneToZeroOne;
                case DXRelationTypeEnum.OneToMany:
                    return DXRelationTypeEnum.ManyToOne;
                case DXRelationTypeEnum.ManyToOne:
                    return DXRelationTypeEnum.OneToMany;
                case DXRelationTypeEnum.ZeroOneToMany:
                    return DXRelationTypeEnum.ManyToZeroOne;
                case DXRelationTypeEnum.ManyToZeroOne:
                    return DXRelationTypeEnum.ZeroOneToMany;
                case DXRelationTypeEnum.ManyToMany:
                    return DXRelationTypeEnum.ManyToMany;
                case DXRelationTypeEnum.ZeroOneToZeroOne:
                    return DXRelationTypeEnum.ZeroOneToZeroOne;
            }

            return value;
        }
    }
}