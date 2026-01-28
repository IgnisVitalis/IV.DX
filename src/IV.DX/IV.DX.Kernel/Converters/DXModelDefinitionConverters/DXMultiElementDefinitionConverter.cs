using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;

namespace IV.DX.Kernel.Converters.DXModelDefinitionConverters
{
    internal static class DXMultiElementDefinitionConverter
    {
        public static DXTableDefinition? ToDXElementDefinition(this DXMultiElement dxMultiElement, string dxUnitTypeName)
        {
            var item = new DXTableDefinition(dxUnitTypeName, dxMultiElement.Attribute.Type, dxMultiElement.Name, dxMultiElement.IsRequired);

            var existingElement = dxMultiElement.Announced.Count() > 0 ? dxMultiElement.Announced.First() 
                : dxMultiElement.Deleted.Count() > 0 ? dxMultiElement.Deleted.First() : null;

            if (existingElement == null)
                return null;

            var propertyNames = existingElement.Content.Select(y => y.Key).ToList();

            if (!propertyNames.Contains(Constants.ID))
            {
                propertyNames.Add(Constants.ID);
            }

            if (!propertyNames.Contains(Constants.DXUnitID))
            {
                propertyNames.Add(Constants.DXUnitID);
            }
            
            if (!propertyNames.Contains(Constants.DXCustomUnitID(dxUnitTypeName)))
            {
                propertyNames.Add(Constants.DXCustomUnitID(dxUnitTypeName));
            }

            item.AddPropertyDefinitions(propertyNames.Select(y => new DXColumnDefinition(y, new DXColumnAttribute(y))).ToList());

            return item;
        }
    }
}