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
                .Where(x => x.ObjectNameLeft == typeName)
                .Where(x =>
                    x.RelationType == DXRelationTypeEnum.ManyToOne
                    || x.RelationType == DXRelationTypeEnum.ManyToZeroOne
                    || x.RelationType == DXRelationTypeEnum.OneToZeroOne
                    || x.RelationType == DXRelationTypeEnum.ZeroOneToOne
                    || x.RelationType == DXRelationTypeEnum.ZeroOneToZeroOne)
                .Where(x => !(x.RelationNameRight.EndsWith(Constants.DXUnitIDSuffix)
                            || x.RelationNameLeft.EndsWith(Constants.DXUnitIDSuffix)));

            return selectedRelations
                .Select(x => new DXPropertyDefinition(x.RelationNameRight, new DXColumnAttribute(x.RelationNameRight)))
                .ToList();
        }
    }
}
