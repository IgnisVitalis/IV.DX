using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;

namespace IV.DX.Kernel.Converters.DXModelDefinitionConverters
{
    internal static class DXSingleElementDefinitionConverter
    {
        public static DXTableDefinition ToDXElementDefinition(this DXSingleElement dxSingleElement, string dxUnitTypeName)
        {
            var item = new DXTableDefinition(dxUnitTypeName, dxSingleElement.Attribute.Type, dxSingleElement.Name, dxSingleElement.IsRequired);

            var propertyNames = dxSingleElement.Item.Content.Select(y => y.Key).ToList();

            if (!propertyNames.Contains(Constants.ID))
            {
                propertyNames.Add(Constants.ID);
            }

            if (!propertyNames.Contains(Constants.DXUnitID))
            {
                propertyNames.Add(Constants.DXUnitID);
            }

            item.AddPropertyDefinitions(propertyNames.Select(y => new DXColumnDefinition(y, new DXColumnAttribute(y))).ToList());

            return item;
        }
    }
}