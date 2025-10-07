using IV.DX.Kernel.Attributes;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Models
{
    public class ESQLSingleItem
    {
        public string Name { get; set; }
        public DXElementAttribute BlockInfo { get; set; }
        public ESQLItem Item { get; set; }

        public JProperty ConvertToJProperty()
        {
            JObject jObject = null;

            if (this.Item != null)
            {   
                jObject = new JObject(this.Item.ConvertToJObject())
                {
                    [Constants.SystemPropertyTypeName] = this.BlockInfo.BlockName
                };
            }

            JProperty jProperty = new JProperty(this.Name, jObject);

            return jProperty;
        }

        public JProperty ConvertToJPropertyWithoutSystemProperties()
        {
            if (this.Item == null)
                return null;

            JObject jObject = new JObject(this.Item.ConvertToJObjectWithoutSystemProperties());

            JProperty jProperty = new JProperty(this.Name, jObject);

            return jProperty;
        }

        public static ESQLSingleItem ConvertFromJProperty(JProperty jProperty)
        {
            if (jProperty == null)
                return null;

            ESQLSingleItem singleFragment = new ESQLSingleItem
            {
                BlockInfo = new DXElementAttribute(jProperty[Constants.SystemPropertyTypeName] != null ? jProperty[Constants.SystemPropertyTypeName].Value<string>() : jProperty.Name),
                Name = jProperty.Name
            };

            var jObjectForContent = jProperty.Value as JObject;
            jObjectForContent.Remove(Constants.SystemPropertyTypeName);

            singleFragment.Item = ESQLItem.ConvertFromJObject(jObjectForContent);

            return singleFragment;
        }

        public static bool DeepEquals(ESQLSingleItem item1, ESQLSingleItem item2)
        {
            if (item1 == null || item2 == null)
                return false;

            var result =
                item1.Name == item2.Name
                && DXElementAttribute.DeepEquals(item1.BlockInfo, item2.BlockInfo)
                && ESQLItem.DeepEquals(item1.Item, item2.Item);

            return result;
        }

        public static bool DeepEquals(IEnumerable<ESQLSingleItem> list1, IEnumerable<ESQLSingleItem> list2)
        {
            if (list1 == null && list2 == null)
                return false;

            if (list1.Count() != list2.Count())
                return false;

            foreach (var item1 in list1)
            {
                var item2 = list2.SingleOrDefault(x => x.Name == item1.Name);

                if (item2 == null)
                    return false;

                if (!ESQLSingleItem.DeepEquals(item1, item2))
                    return false;
            }

            return true;
        }

        public ESQLSingleItem DeepClone()
        {
            return new ESQLSingleItem()
            {
                BlockInfo = this.BlockInfo?.DeepClone(),
                Name = this.Name,
                Item = this.Item.DeepClone()
            };
        }
    }
}