using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters.DXModelDefinitionConverters;
using IV.DX.Kernel.Models;

namespace IV.DX.Kernel.Helpers.DXModelDefinitionHelpers
{
    internal static class DXModelDefinitionHelper
    {
        public static DXModelDefinition BuildModelDefinition(DXEnumDefinitionUnit mainDXUnit, IEnumerable<DXRelationDefinitionUnit> relations)
        {
            var dxModel = BuildBaseModelDefinition(mainDXUnit, Enumerable.Empty<DXObjectDefinitionUnit>(), relations);

            return dxModel;
        }

        private static DXModelDefinition BuildBaseModelDefinition(
            DXObjectDefinitionUnit dxUnit,
            IEnumerable<DXObjectDefinitionUnit> baseDXUnits,
            IEnumerable<DXRelationDefinitionUnit> relations)
        {
            if (dxUnit == null)
                return null;

            var ownDXElementDefinition = new DXElementDefinition(dxUnit.Name, dxUnit.Name, true);

            List<DXPropertyDefinition> props = new List<DXPropertyDefinition>();


            var dxUnitProps = dxUnit.DXColumnDefinitionElement.Announced.Select(x => new DXPropertyDefinition(x.Name, new DXColumnAttribute(x.Name)));

            props.AddRange(dxUnitProps);

            foreach (var baseDXUnit in baseDXUnits)
            {
                var baseDXUnitProps = baseDXUnit.DXColumnDefinitionElement.Announced.Select(x => new DXPropertyDefinition(x.Name, new DXColumnAttribute(x.Name)));

                props.AddRange(baseDXUnitProps);
            }         

            //var relationsAsProperties = relations.ToDXPropertyDefinitions(mainDXObject.Name);

            ownDXElementDefinition.AddPropertyDefinitions(props);
            //ownDXElementDefinition.AddPropertyDefinitions(relationsAsProperties);

            var dxModel = new DXModelDefinition(ownDXElementDefinition);

            return dxModel;
        }

        public static DXModelDefinition BuildModelDefinition(
            DXUnitDefinitionUnit dxUnit,
            IEnumerable<DXUnitDefinitionUnit> baseDXUnits,
            IEnumerable<DXRelationDefinitionUnit> relations,
            IEnumerable<DXElementDefinitionUnit> relatedSingleMandatoryDXElements = null,
            IEnumerable<DXElementDefinitionUnit> relatedSingleOptionalDXElements = null,
            IEnumerable<DXElementDefinitionUnit> relatedMultiMandatoryDXElements = null,
            IEnumerable<DXElementDefinitionUnit> relatedMultiOptionalDXElements = null)
        {
            var dxModel = BuildBaseModelDefinition(dxUnit, baseDXUnits, relations);

            if (dxModel == null)
                return null;

            var singleDXElements = new List<DXElementDefinition>();
            var multiDXElements = new List<DXElementDefinition>();

            if (relatedSingleMandatoryDXElements != null)
            {
                var definitions = relatedSingleMandatoryDXElements.Select(x => DXElementDefinitionConverter.ToDXElementDefinition(x, relations, true));

                singleDXElements.AddRange(definitions);
            }

            if (relatedSingleOptionalDXElements != null)
            {
                var definitions = relatedSingleOptionalDXElements.Select(x => DXElementDefinitionConverter.ToDXElementDefinition(x, relations, false));

                singleDXElements.AddRange(definitions);
            }

            if (relatedMultiMandatoryDXElements != null)
            {
                var definitions = relatedMultiMandatoryDXElements.Select(x => DXElementDefinitionConverter.ToDXElementDefinition(x, relations, true));

                multiDXElements.AddRange(definitions);
            }

            if (relatedMultiOptionalDXElements != null)
            {
                var definitions = relatedMultiOptionalDXElements.Select(x => DXElementDefinitionConverter.ToDXElementDefinition(x, relations, false));

                multiDXElements.AddRange(definitions);
            }

            if (singleDXElements.Count > 0)
            {
                dxModel.AddToSingleItemDefinitions(singleDXElements);
            }

            if (multiDXElements.Count > 0)
            {
                dxModel.AddToMultiItemDefinitions(multiDXElements);
            }

            return dxModel;
        }
    }
}
