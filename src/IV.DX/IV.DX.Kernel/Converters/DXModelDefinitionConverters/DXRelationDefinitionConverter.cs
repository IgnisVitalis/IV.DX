using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;

namespace IV.DX.Kernel.Converters.DXModelDefinitionConverters
{
    internal static class DXRelationDefinitionConverter
    {
        public static IEnumerable<DXPropertyDefinition> ToDXPropertyDefinitions(this IEnumerable<DXRelationDefinitionUnit> relations, string typeName)
        {
            var selectedRelations = relations
                .Where(x => x.DXRelationDefinitionMainElement.ObjectNameLeft == typeName)
                .Where(x =>
                    x.DXRelationDefinitionMainElement.RelationType == DXRelationTypeEnum.ManyToOne
                    || x.DXRelationDefinitionMainElement.RelationType == DXRelationTypeEnum.ManyToZeroOne
                    || x.DXRelationDefinitionMainElement.RelationType == DXRelationTypeEnum.OneToZeroOne
                    || x.DXRelationDefinitionMainElement.RelationType == DXRelationTypeEnum.ZeroOneToOne
                    || x.DXRelationDefinitionMainElement.RelationType == DXRelationTypeEnum.ZeroOneToZeroOne)
                .Where(x => !(x.DXRelationDefinitionMainElement.RelationNameRight.EndsWith(Constants.DXUnitIDSuffix)
                            || x.DXRelationDefinitionMainElement.RelationNameLeft.EndsWith(Constants.DXUnitIDSuffix)));

            return selectedRelations
                .Select(x => new DXPropertyDefinition(x.DXRelationDefinitionMainElement.RelationNameRight, new DXColumnAttribute(x.DXRelationDefinitionMainElement.RelationNameRight)))
                .ToList();
        }
    }
}
