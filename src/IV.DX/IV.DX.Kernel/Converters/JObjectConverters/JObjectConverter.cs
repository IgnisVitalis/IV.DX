using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters.DXObjectConverters;
using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Converters.JObjectConverters
{
    internal static class JObjectConverter
    {
        public static JObject ToJObject(this DXUnit dxUnit) =>
           JObject.FromObject(DXRecordWriter.ToBlock(dxUnit));


        public static JObject ToJObject(this DXElement? dxElement)
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

    }
}
