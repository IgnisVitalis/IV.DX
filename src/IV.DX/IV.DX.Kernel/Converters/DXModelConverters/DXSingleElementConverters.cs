using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Converters.DXModelConverters
{
    internal static class DXSingleElementConverters
    {
        public static DXSingleElement ToDXSingleElement(this DXElement dxElement, string propertyName = null)
        {
            if (dxElement.DXUnitID == default)
                throw new Exception($"DXElement should have DXUnitID value");

            var content = dxElement.ToJObject();

            var attribute = DXReflectionHelper.GetAttr<DXElementAttribute>(dxElement.GetType());
            var name = propertyName ?? attribute.Type;            
            var item = new DXItem
            {
                ID = dxElement.ID,
                DXUnitID = dxElement.DXUnitID,
                Content = content
            };

            return new DXSingleElement(name, attribute, item);
        }

        public static DXSingleElement ToDXSingleElement(this JProperty jProperty)
        {
            if (jProperty == null)
                return null;

            var name = jProperty.Name;
            var attribute  = new DXElementAttribute(jProperty[Constants.SystemPropertyTypeName] != null ? jProperty[Constants.SystemPropertyTypeName].Value<string>() : jProperty.Name);

            var jObjectForContent = jProperty.Value as JObject;
            jObjectForContent.Remove(Constants.SystemPropertyTypeName);

            var item = jObjectForContent.ToDXItem();

            DXSingleElement singleFragment = new DXSingleElement(name, attribute, item);         

            return singleFragment;
        }
    }
}
