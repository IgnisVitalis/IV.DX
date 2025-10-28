using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Converters
{
    internal static class DictionaryConverter
    {
        public static IDictionary<string, object> ToDictionary(this JObject jObject)
        {
            var result = new Dictionary<string, object>();

            foreach (var property in jObject.Properties())
            {
                result[property.Name] = ConvertToken(property.Value);
            }

            return result;
        }

        public static IDictionary<string, object> ToDictionary(this DXElement? dxElement)
        {
            if (dxElement is null) return null;

            var dict = new Dictionary<string, object>();

            var elementInfo = DXReflectionHelper.GetAttr<DXElementAttribute>(dxElement.GetType());

            dict[Constants.SystemPropertyTypeName] = elementInfo.Type;

            foreach (var prop in DXReflectionHelper.GetPropsWithAttribute<DXColumnAttribute>(dxElement.GetType()))
            {
                var value = prop.GetValue(dxElement);
                dict[prop.Name] = value;
            }

            return dict;
        }

        private static object ConvertToken(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    return ToDictionary((JObject)token);
                case JTokenType.Array:
                    var list = new List<object>();
                    foreach (var item in (JArray)token)
                    {
                        list.Add(ConvertToken(item));
                    }
                    return list;
                case JTokenType.Null:
                    return null;
                default:
                    return ((JValue)token).Value;
            }
        }
    }
}