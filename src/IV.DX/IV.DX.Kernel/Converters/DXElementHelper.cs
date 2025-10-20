using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Converters
{
    internal static class DXElementHelper
    {
        public static JObject? GetContent(this DXElement? dxElement)
        {
            if (dxElement is null) return null;

            var jObject = new JObject();
            foreach (var prop in DXReflectionHelper.GetPropsWithAttribute<DXColumnAttribute>(dxElement.GetType()))
            {
                var value = prop.GetValue(dxElement);
                jObject[prop.Name] = new JValue(value);
            }
            return jObject;
        }

        public static JObject ConvertToJObject(this DXElement dxElement)
        {
            var jObject = dxElement.GetContent() ?? new JObject();
            var elementInfo = DXReflectionHelper.GetAttr<DXElementAttribute>(dxElement.GetType());
            if (elementInfo != null)
            {
                jObject[Constants.SystemPropertyTypeName] = elementInfo.Name;
            }
            return jObject;
        }

        public static DXSingleElement? ConvertToDXSingleElement(this DXElement? dxElement)
        {
            if (dxElement is null) return null;

            var elementInfo = DXReflectionHelper.GetAttr<DXElementAttribute>(dxElement.GetType());
            return new DXSingleElement
            {
                ElementInfo = elementInfo,
                Item = new DXItem
                {
                    ID = dxElement.ID,
                    ObjectID = dxElement.ObjectID,
                    Content = dxElement.ConvertToJObject()
                },
                Name = elementInfo?.Name
            };
        }

        public static DXSingleElement ConvertToSingleItem(this DXElement dxElement)
        {
            var dxElementInfo = DXReflectionHelper.GetAttr<DXElementAttribute>(dxElement.GetType());
            return new DXSingleElement
            {
                ElementInfo = dxElementInfo,
                Item = new DXItem
                {
                    ID = dxElement.ID,
                    ObjectID = dxElement.ObjectID,
                    Content = dxElement.GetContent()
                },
                Name = dxElementInfo?.Name
            };
        }
    }
}
