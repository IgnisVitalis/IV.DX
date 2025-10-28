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

            var dxElementInfo = DXReflectionHelper.GetAttr<DXElementAttribute>(dxElement.GetType());

            var content = dxElement.ToJObject();

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

        public static DXSingleElement ToDXSingleElement(this JProperty jProperty)
        {
            if (jProperty == null)
                return null;

            DXSingleElement singleFragment = new DXSingleElement
            {
                Attribute = new DXElementAttribute(jProperty[Constants.SystemPropertyTypeName] != null ? jProperty[Constants.SystemPropertyTypeName].Value<string>() : jProperty.Name),
                Name = jProperty.Name
            };

            var jObjectForContent = jProperty.Value as JObject;
            jObjectForContent.Remove(Constants.SystemPropertyTypeName);

            singleFragment.Item = jObjectForContent.ToDXItem();

            return singleFragment;
        }
    }
}
