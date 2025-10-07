using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters;
using IV.DX.Kernel.Enums;

namespace IV.DX.Kernel.Models
{
    [ESQLObjectDefinition("DXRelationDefinitionUnit")]
    public class DXRelationDefinitionUnit : ESQLObject
    {
        public static ESQLModelDefinition ESQLModelDefinition { get; } = ModelConverter.GetESQLModelDefinition<DXRelationDefinitionUnit>();

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

        private DPRelationTypeEnum GetInvertedRelationType(DPRelationTypeEnum value)
        {
            switch (value)
            {
                case DPRelationTypeEnum.OneToZeroOne:
                    return DPRelationTypeEnum.ZeroOneToOne;
                case DPRelationTypeEnum.ZeroOneToOne:
                    return DPRelationTypeEnum.OneToZeroOne;
                case DPRelationTypeEnum.OneToMany:
                    return DPRelationTypeEnum.ManyToOne;
                case DPRelationTypeEnum.ManyToOne:
                    return DPRelationTypeEnum.OneToMany;
                case DPRelationTypeEnum.ZeroOneToMany:
                    return DPRelationTypeEnum.ManyToZeroOne;
                case DPRelationTypeEnum.ManyToZeroOne:
                    return DPRelationTypeEnum.ZeroOneToMany;
                case DPRelationTypeEnum.ManyToMany:
                    return DPRelationTypeEnum.ManyToMany;
                case DPRelationTypeEnum.ZeroOneToZeroOne:
                    return DPRelationTypeEnum.ZeroOneToZeroOne;
            }

            return value;
        }
    }
}