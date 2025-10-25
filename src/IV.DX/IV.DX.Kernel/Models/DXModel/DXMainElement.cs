using IV.DX.Kernel.Attributes;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Models
{
    public class DXMainElement
    {
        public DXUnitAttribute ObjectInfo { get; private set; }
        public DXItem Item { get; set; }

        public DXMainElement(DXUnitAttribute objectInfo)
        {
            this.ObjectInfo = objectInfo;
        }

        public static bool DeepEquals(DXMainElement item1, DXMainElement item2)
        {
            if (item1 == null || item2 == null)
                return false;

            var result =
                DXUnitAttribute.DeepEquals(item1.ObjectInfo, item2.ObjectInfo)
                && DXItem.DeepEquals(item1.Item, item2.Item);

            return result;
        }
        public DXMainElement DeepClone()
        {
            return new DXMainElement(this.ObjectInfo.DeepClone())
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