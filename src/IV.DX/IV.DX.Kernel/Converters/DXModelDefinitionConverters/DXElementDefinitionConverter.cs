using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;

namespace IV.DX.Kernel.Converters.DXModelDefinitionConverters
{
    internal static class DXElementDefinitionConverter
    {
        public static DXElementDefinition ConvertToDXElementDefinition(DXElementDefinitionUnit dxElement)
        {
            var props = dxElement.DXColumnDefinitionElement.Announced
                           .Select(y => new DXPropertyDefinition(y.Name, new DXColumnAttribute(y.Name)));

            var singleFragmentDefinition = new DXElementDefinition(dxElement.DXObjectDefinitionMainElement.Name, dxElement.DXObjectDefinitionMainElement.Name);

            singleFragmentDefinition.AddPropertyDefinitions(props);

            return singleFragmentDefinition;
        }
    }
}
