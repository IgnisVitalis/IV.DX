using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;
using System.Linq.Expressions;

namespace IV.DX.Kernel.Converters
{
    internal static class DXElementHelper
    {
        public static JObject GetContent(this DXElement dxElement)
        {
            if (dxElement == null)
                return null;

            JObject jObject = new JObject();
            
            var properties = dxElement.GetType().GetProperties()
                .Where(x => AttributeReader.GetAttribute<DXColumnAttribute>(x) != null)
                .ToList();       

            foreach (var property in properties)
            {
                var attribute = AttributeReader.GetAttribute<DXColumnAttribute>(property);

                jObject[property.Name] = new JValue(property.GetValue(dxElement));
            }

            return jObject;
        }

        public static JObject ConvertToJObject(this DXElement dxElement)
        {
            var jObject = dxElement.GetContent();

            var elementInfo = AttributeReader.GetAttribute<DXElementAttribute>(dxElement.GetType());

            jObject[Constants.SystemPropertyTypeName] = elementInfo.Name;

            return jObject;
        }          

        public static DXSingleElement? ConvertToDXSingleElement(this DXElement dxElement)
        {
            var elementInfo = AttributeReader.GetAttribute<DXElementAttribute>(dxElement.GetType());

            DXSingleElement dxSingleItem = new DXSingleElement()
            {
                ElementInfo = elementInfo,

                Item = new DXItem()
                {
                    ID = dxElement?.ID,
                    ObjectID = dxElement.ObjectID,
                    Content = ConvertToJObject(dxElement),
                },
                Name = elementInfo.Name
            };

            return dxSingleItem;
        }
    }
}
