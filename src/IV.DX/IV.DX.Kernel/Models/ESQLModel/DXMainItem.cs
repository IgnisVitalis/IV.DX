using IV.DX.Kernel.Attributes;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Models
{
    public class DXMainItem
    {
        public DXUnitAttribute ObjectInfo { get; private set; }
        public DXItem Item { get; set; }

        public DXMainItem(DXUnitAttribute objectInfo)
        {
            this.ObjectInfo = objectInfo;
        }

        public static bool DeepEquals(DXMainItem item1, DXMainItem item2)
        {
            if (item1 == null || item2 == null)
                return false;

            var result =
                DXUnitAttribute.DeepEquals(item1.ObjectInfo, item2.ObjectInfo)
                && DXItem.DeepEquals(item1.Item, item2.Item);

            return result;
        }
        public DXMainItem DeepClone()
        {
            return new DXMainItem(this.ObjectInfo.DeepClone())
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