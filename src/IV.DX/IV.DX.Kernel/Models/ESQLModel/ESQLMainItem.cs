using IV.DX.Kernel.Attributes;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Models
{
    public class ESQLMainItem
    {
        public ESQLObjectDefinitionAttribute ObjectInfo { get; private set; }
        public ESQLItem Item { get; set; }

        public ESQLMainItem(ESQLObjectDefinitionAttribute objectInfo)
        {
            this.ObjectInfo = objectInfo;
        }

        public static bool DeepEquals(ESQLMainItem item1, ESQLMainItem item2)
        {
            if (item1 == null || item2 == null)
                return false;

            var result =
                ESQLObjectDefinitionAttribute.DeepEquals(item1.ObjectInfo, item2.ObjectInfo)
                && ESQLItem.DeepEquals(item1.Item, item2.Item);

            return result;
        }
        public ESQLMainItem DeepClone()
        {
            return new ESQLMainItem(this.ObjectInfo.DeepClone())
            {
                Item = this.Item.DeepClone(),
            };
        }

        public JProperty ConvertToJPropertyWithoutSystemProperties()
        {
            JObject jObject = new JObject(this.Item.ConvertToJObjectWithoutSystemProperties());

            JProperty jProperty = new JProperty(this.ObjectInfo.ObjectName, jObject);

            return jProperty;
        }
    }
}