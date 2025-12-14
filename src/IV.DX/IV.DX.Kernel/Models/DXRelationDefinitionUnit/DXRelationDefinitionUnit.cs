using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXRelationDefinitionUnit")]
    public class DXRelationDefinitionUnit : DXUnit
    {
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
                    DXUnitID = objectId,
                    ObjectNameLeft = this.DXRelationDefinitionMainElement.ObjectNameRight,
                    ObjectNameRight = this.DXRelationDefinitionMainElement.ObjectNameLeft,
                    RelationNameLeft = this.DXRelationDefinitionMainElement.RelationNameRight,
                    RelationNameRight = this.DXRelationDefinitionMainElement.RelationNameLeft,
                    RelationTable = this.DXRelationDefinitionMainElement.RelationTable,
                    RelationType = DXRelationTypeEnumHelper.GetInvertedRelationType(this.DXRelationDefinitionMainElement.RelationType),
                    RelationColumnNameLeft = this.DXRelationDefinitionMainElement.RelationColumnNameRight,
                    RelationColumnNameRight = this.DXRelationDefinitionMainElement.RelationColumnNameLeft,
                    RelationColumnTypeLeft = this.DXRelationDefinitionMainElement.RelationColumnTypeRight,
                    RelationColumnTypeRight = this.DXRelationDefinitionMainElement.RelationColumnTypeLeft,
                    Kind = this.DXRelationDefinitionMainElement.Kind
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
    }
}