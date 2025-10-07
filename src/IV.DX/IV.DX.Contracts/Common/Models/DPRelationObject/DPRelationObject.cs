using IV.DataProvider.Persistence.Contracts.Models;
using IV.DX.Contracts.Common.Attributes;
using IV.DX.Contracts.Common.Converters;
using IV.DX.Contracts.Common.Enums;

namespace IV.DX.Contracts.Common.Models
{
    [ESQLObjectDefinition("DPRelationObject")]
    public class DPRelationObject : ESQLObject
    {
        public static ESQLModelDefinition ESQLModelDefinition { get; } = ModelConverter.GetESQLModelDefinition<DPRelationObject>();

        public DPRelationGenBlock DPRelationGenBlock { get; set; }

        public DPRelationObject CreateInvertedRelationObject()
        {
            var objectId = Guid.NewGuid();

            return new DPRelationObject()
            {
                ID = objectId,
                DPRelationGenBlock = new DPRelationGenBlock()
                {
                    ID = Guid.NewGuid(),
                    ObjectID = objectId,
                    ObjectNameLeft = this.DPRelationGenBlock.ObjectNameRight,
                    ObjectNameRight = this.DPRelationGenBlock.ObjectNameLeft,
                    RelationNameLeft = this.DPRelationGenBlock.RelationNameRight,
                    RelationNameRight = this.DPRelationGenBlock.RelationNameLeft,
                    RelationTable = this.DPRelationGenBlock.RelationTable,
                    RelationType = this.GetInvertedRelationType(this.DPRelationGenBlock.RelationType),
                    RelationColumnNameLeft = this.DPRelationGenBlock.RelationColumnNameRight,
                    RelationColumnNameRight = this.DPRelationGenBlock.RelationColumnNameLeft,
                    RelationColumnTypeLeft = this.DPRelationGenBlock.RelationColumnTypeRight,
                    RelationColumnTypeRight = this.DPRelationGenBlock.RelationColumnTypeLeft
                }
            };
        }

        public string GetQueryForInvertedRelationObject()
        {
            return $@"DPRelationGenBlock.ObjectNameRight = '{this.DPRelationGenBlock.ObjectNameLeft}' 
                    AND DPRelationGenBlock.ObjectNameLeft = '{this.DPRelationGenBlock.ObjectNameRight}'
                    AND DPRelationGenBlock.RelationNameRight = '{this.DPRelationGenBlock.RelationNameLeft}'
                    AND DPRelationGenBlock.RelationNameLeft = '{this.DPRelationGenBlock.RelationNameRight}'";
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