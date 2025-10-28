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

            var ownDXElementDefinition = new DXElementDefinition(mainDXObject.DXObjectDefinitionMainElement.Name, mainDXObject.DXObjectDefinitionMainElement.Name);

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

            var singleDXElements = new List<DXElementDefinitionUnit>();
            var multiDXElements = new List<DXElementDefinitionUnit>();

            if (relatedSingleMandatoryDXElements != null)
            {
                singleDXElements.AddRange(relatedSingleMandatoryDXElements);
            }

            if (relatedSingleOptionalDXElements != null)
            {
                singleDXElements.AddRange(relatedSingleOptionalDXElements);
            }

            if (relatedMultiMandatoryDXElements != null)
            {
                multiDXElements.AddRange(relatedMultiMandatoryDXElements);
            }

            if (relatedMultiOptionalDXElements != null)
            {
                multiDXElements.AddRange(relatedMultiOptionalDXElements);
            }

            if (singleDXElements.Count > 0)
            {
                dxModel.AddToSingleItemDefinitions(singleDXElements.Select(x => DXElementDefinitionConverter.ConvertToDXElementDefinition(x)).ToHashSet());
            }

            if (multiDXElements.Count > 0)
            {
                dxModel.AddToMultiItemDefinitions(multiDXElements.Select(x => DXElementDefinitionConverter.ConvertToDXElementDefinition(x)).ToHashSet());
            }

            return dxModel;
        }
    }
}
