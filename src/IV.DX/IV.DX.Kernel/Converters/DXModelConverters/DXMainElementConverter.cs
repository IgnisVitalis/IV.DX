using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Converters.DXModelConverters
{
    internal static class DXMainElementConverter
    {
        public static JProperty ConvertToJPropertyWithoutSystemProperties(this DXMainElement mainElement)
        {
            JObject jObject = new JObject(mainElement.Item.Parse(true));

            JProperty jProperty = new JProperty(mainElement.Attribute.Type, jObject);

            return jProperty;
        }
    }
}
