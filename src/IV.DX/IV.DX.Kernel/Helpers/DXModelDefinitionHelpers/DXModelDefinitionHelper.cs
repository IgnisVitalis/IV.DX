using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters.DXModelDefinitionConverters;
using IV.DX.Kernel.Models;

namespace IV.DX.Kernel.Helpers.DXModelDefinitionHelpers
{
    internal static class DXModelDefinitionHelper
    {
        public static DXModelDefinition BuildModelDefinition(DXEnumDefinitionUnit mainDXUnit)
        {
            var dxModel = BuildBaseModelDefinition(mainDXUnit);

            return dxModel;
        }

        private static DXModelDefinition BuildBaseModelDefinition(DXObjectDefinitionUnit mainDXObject)
        {
            if (mainDXObject == null)
                return null;

            var ownDXElementDefinition = new DXElementDefinition(mainDXObject.DXObjectDefinitionMainElement.Name, mainDXObject.DXObjectDefinitionMainElement.Name, true);

            var props = mainDXObject.DXColumnDefinitionElement.Announced?.Select(x => new DXPropertyDefinition(x.Name, new DXColumnAttribute(x.Name)));

            ownDXElementDefinition.AddPropertyDefinitions(props);

            var dxModel = new DXModelDefinition(ownDXElementDefinition);

            return dxModel;
        }

        public static DXModelDefinition BuildModelDefinition(
            DXUnitDefinitionUnit mainDXUnit,
            IEnumerable<DXElementDefinitionUnit> relatedSingleMandatoryDXElements = null,
            IEnumerable<DXElementDefinitionUnit> relatedSingleOptionalDXElements = null,
            IEnumerable<DXElementDefinitionUnit> relatedMultiMandatoryDXElements = null,
            IEnumerable<DXElementDefinitionUnit> relatedMultiOptionalDXElements = null)
        {
            var dxModel = BuildBaseModelDefinition(mainDXUnit);

            if (dxModel == null)
                return null;

            var singleDXElements = new List<DXElementDefinition>();
            var multiDXElements = new List<DXElementDefinition>();

            if (relatedSingleMandatoryDXElements != null)
            {
                var definitions = relatedSingleMandatoryDXElements.Select(x => DXElementDefinitionConverter.ToDXElementDefinition(x, true));

                singleDXElements.AddRange(definitions);
            }

            if (relatedSingleOptionalDXElements != null)
            {
                var definitions = relatedSingleOptionalDXElements.Select(x => DXElementDefinitionConverter.ToDXElementDefinition(x, false));

                singleDXElements.AddRange(definitions);
            }

            if (relatedMultiMandatoryDXElements != null)
            {
                var definitions = relatedMultiMandatoryDXElements.Select(x => DXElementDefinitionConverter.ToDXElementDefinition(x, true));

                multiDXElements.AddRange(definitions);
            }

            if (relatedMultiOptionalDXElements != null)
            {
                var definitions = relatedMultiOptionalDXElements.Select(x => DXElementDefinitionConverter.ToDXElementDefinition(x, false));

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
