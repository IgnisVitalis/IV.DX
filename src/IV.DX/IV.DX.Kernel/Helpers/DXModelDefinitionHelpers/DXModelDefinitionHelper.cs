using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters.DXModelDefinitionConverters;
using IV.DX.Kernel.Models;

namespace IV.DX.Kernel.Helpers.DXModelDefinitionHelpers
{
    internal static class DXModelDefinitionHelper
    {
        public static DXDataSetDefinition BuildModelDefinition(DXEnumDefinitionUnit mainDXUnit, IEnumerable<DXRelationDefinitionUnit> relations)
        {
            var dxModel = BuildBaseDataSetDefinition(mainDXUnit, Enumerable.Empty<DXObjectDefinitionUnit>(), relations);

            return dxModel;
        }

        private static DXDataSetDefinition BuildBaseDataSetDefinition(
            DXObjectDefinitionUnit dxUnit,
            IEnumerable<DXObjectDefinitionUnit> baseDXUnits,
            IEnumerable<DXRelationDefinitionUnit> relations)
        {
            if (dxUnit == null)
                return null;

            var ownDXElementDefinition = new DXMainTableDefinition(dxUnit.Name, dxUnit.Name, true);

            List<DXColumnDefinition> props = new List<DXColumnDefinition>();


            var dxUnitProps = dxUnit.DXColumnDefinitionElement.Announced.Select(x => new DXColumnDefinition(x.Name, new DXColumnAttribute(x.Name)));

            props.AddRange(dxUnitProps);

            foreach (var baseDXUnit in baseDXUnits)
            {
                var baseDXUnitProps = baseDXUnit.DXColumnDefinitionElement.Announced.Select(x => new DXColumnDefinition(x.Name, new DXColumnAttribute(x.Name)));

                props.AddRange(baseDXUnitProps);
            }

            //var relationsAsProperties = relations.ToDXPropertyDefinitions(mainDXObject.Name);

            ownDXElementDefinition.AddPropertyDefinitions(props);
            //ownDXElementDefinition.AddPropertyDefinitions(relationsAsProperties);

            var dxModel = new DXDataSetDefinition(ownDXElementDefinition);

            return dxModel;
        }

        public static DXDataSetDefinition BuildModelDefinition(
            DXUnitDefinitionUnit dxUnit,
            IEnumerable<DXUnitDefinitionUnit> baseDXUnits,
            IEnumerable<DXRelationDefinitionUnit> relations,
            IEnumerable<DXElementDefinitionUnit> relatedSingleMandatoryDXElements = null,
            IEnumerable<DXElementDefinitionUnit> relatedSingleOptionalDXElements = null,
            IEnumerable<DXElementDefinitionUnit> relatedMultiMandatoryDXElements = null,
            IEnumerable<DXElementDefinitionUnit> relatedMultiOptionalDXElements = null)
        {
            var dxModel = BuildBaseDataSetDefinition(dxUnit, baseDXUnits, relations);

            if (dxModel == null)
                return null;

            var singleDXElements = new List<DXTableDefinition>();
            var multiDXElements = new List<DXTableDefinition>();

            if (relatedSingleMandatoryDXElements != null)
            {
                var definitions = relatedSingleMandatoryDXElements.Select(x => x.ToDXTableDefinition(dxUnit.Name, relations, true));

                singleDXElements.AddRange(definitions);
            }

            if (relatedSingleOptionalDXElements != null)
            {
                var definitions = relatedSingleOptionalDXElements.Select(x => x.ToDXTableDefinition(dxUnit.Name, relations, false));

                singleDXElements.AddRange(definitions);
            }

            if (relatedMultiMandatoryDXElements != null)
            {
                var definitions = relatedMultiMandatoryDXElements.Select(x => x.ToDXTableDefinition(dxUnit.Name, relations, true));

                multiDXElements.AddRange(definitions);
            }

            if (relatedMultiOptionalDXElements != null)
            {
                var definitions = relatedMultiOptionalDXElements.Select(x => x.ToDXTableDefinition(dxUnit.Name, relations, false));

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
