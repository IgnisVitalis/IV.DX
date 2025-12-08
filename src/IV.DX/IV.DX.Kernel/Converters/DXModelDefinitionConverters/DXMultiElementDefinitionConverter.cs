using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;

namespace IV.DX.Kernel.Converters.DXModelDefinitionConverters
{
    internal static class DXMultiElementDefinitionConverter
    {
        public static DXElementDefinition? ToDXElementDefinition(this DXMultiElement dxMultiElement)
        {
            var item = new DXElementDefinition(dxMultiElement.Attribute.Type, dxMultiElement.Name, dxMultiElement.IsRequired);

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

            item.AddPropertyDefinitions(propertyNames.Select(y => new DXPropertyDefinition(y, new DXColumnAttribute(y))).ToList());

            return item;
        }
    }
}