using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Converters.DXModelConverters
{
    internal static class DXSingleElementConverters
    {
        public static DXSingleElement ToDXSingleElement(this DXElement dxElement, bool isRequired, string propertyName = null)
        {
            if (dxElement.DXUnitID == default)
                throw new Exception($"DXElement should have DXUnitID value");

            var content = dxElement.ToDictionary();

            var attribute = DXReflectionHelper.GetAttr<DXElementAttribute>(dxElement.GetType());
            var name = propertyName ?? attribute.Type;

            var item = new DXItem(attribute.Type, dxElement.ID, dxElement.DXUnitID, dxElement.TimeStamp, content);

            return new DXSingleElement(name, attribute, item, isRequired);
        }

        // public static DXSingleElement ToDXSingleElement(this JObject jObject, bool isRequired, string propertyName = null)
        // {
        //     var item = jObject.FromDXUnitToDXItem();
        // }
    }
}