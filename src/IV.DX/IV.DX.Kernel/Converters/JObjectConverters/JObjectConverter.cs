using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters.DXModelConverters;
using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Converters.JObjectConverters
{
    internal static class JObjectConverter
    {
        public static JObject ToJObject(this DXUnit dxUnit) =>
           DXModelConverter.ToDXModel(dxUnit).ToJObject();


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

        public static JObject ToJObject(this DXItem dxItem, bool exlcudeSystemProperties = false)
        {
            JObject jObject = dxItem.Content != null ? new JObject(dxItem.Content) : new JObject();

            if (exlcudeSystemProperties)
            {
                var systemProperties = jObject.Properties().Where(x =>
                       x.Name.Length >= Constants.SystemPropertyPrefix.Length
                       && x.Name.Substring(0, Constants.SystemPropertyPrefix.Length) == Constants.SystemPropertyPrefix
                   ).ToList();

                foreach (var systemProperty in systemProperties)
                {
                    jObject.Remove(systemProperty.Name);
                }
            }

            return jObject;
        }

        public static JObject ToJObject(this DXModel dxModel)
        {
            JObject result = dxModel.DXMainElement.Item.Content.DeepClone() as JObject;

            if (dxModel.DXSingleElements != null)
            {
                foreach (var item in dxModel.DXSingleElements)
                {
                    result.Add(item.ConvertToJProperty());
                }
            }

            if (dxModel.DXMultiElements != null)
            {
                foreach (var item in dxModel.DXMultiElements)
                {
                    result.Add(item.ToJProperty());
                }
            }

            return result;
        }
    }
}