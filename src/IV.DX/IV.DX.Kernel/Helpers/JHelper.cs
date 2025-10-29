using Newtonsoft.Json.Linq;
using System.Globalization;

namespace IV.DX.Kernel.Helpers
{
    internal static class JHelper
    {
        static JToken Normalize(JToken t) =>
            t switch
            {
                JObject o => new JObject(o.Properties().Select(p => new JProperty(p.Name, Normalize(p.Value)))),
                JArray a => new JArray(a.Select(Normalize)),
                JValue v => v.Type switch
                {
                    JTokenType.Guid => new JValue(v.Value<Guid>().ToString("D")),
                    JTokenType.Date => new JValue(v.Value<DateTime>().ToUniversalTime().ToString("o", CultureInfo.InvariantCulture)),
                    _ => new JValue(v.ToString(CultureInfo.InvariantCulture))
                },
                _ => t
            };


        public static bool DeepEquals(JToken t1, JToken t2)
        {
            return JToken.DeepEquals(Normalize(t1), Normalize(t2));
        }

        public static T? GetValue<T>(JObject jObject, string propertyName)
        {
            if (jObject.ContainsKey(propertyName))
                return jObject.Value<T>(propertyName);
            return default(T?);
        }
    }
}