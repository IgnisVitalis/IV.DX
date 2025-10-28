using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Converters
{
    internal static class DXElementHelper
    {
        public static JObject ConvertToJObject(this DXElement? dxElement)
        {
            if (dxElement is null) return null;

            var jObject = new JObject();

            var elementInfo = DXReflectionHelper.GetAttr<DXElementAttribute>(dxElement.GetType());

            jObject[Constants.SystemPropertyTypeName] = elementInfo.Type;

            foreach (var prop in DXReflectionHelper.GetPropsWithAttribute<DXColumnAttribute>(dxElement.GetType()))
            {
                var value = prop.GetValue(dxElement);
                jObject[prop.Name] = new JValue(value);
            }

            return jObject;
        }

        public static DXSingleElement ConvertToSingleItem(this DXElement dxElement, string propertyName = null)
        {
            if (dxElement.DXUnitID == default)
                throw new Exception($"DXElement should have DXUnitID value");

            var dxElementInfo = DXReflectionHelper.GetAttr<DXElementAttribute>(dxElement.GetType());

            var content = dxElement.ConvertToJObject();

            return new DXSingleElement
            {
                Attribute = dxElementInfo,
                Item = new DXItem
                {
                    ID = dxElement.ID,
                    DXUnitID = dxElement.DXUnitID,
                    Content = content
                },
                Name = propertyName ?? dxElementInfo.Type
            };
        }
    }
}
