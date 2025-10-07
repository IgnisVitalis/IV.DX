using IV.DX.Kernel.Attributes;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Models
{
    public class ESQLMultiItem
    {
        public string Name { get; set; }
        public ESQLBlockDefinitionAttribute BlockInfo { get; set; }
        public ModeForMultiItems Mode { get; set; }
        public IEnumerable<ESQLItem> Announced { get; set; }
        public IEnumerable<ESQLItem> Deleted { get; set; }

        public JProperty ConvertToJProperty()
        {
            JObject jObject = new JObject
            {
                [Constants.SystemPropertyTypeName] = this.BlockInfo.BlockName,
                [Constants.Mode] = (int)this.Mode
            };

            JArray announced = new JArray();
            JArray Deleted = new JArray();

            if (this.Announced != null)
            {
                foreach (var item in this.Announced)
                {
                    announced.Add(item.ConvertToJObject());
                }
            }

            if (this.Deleted != null)
            {
                foreach (var item in this.Deleted)
                {
                    Deleted.Add(item.ConvertToJObject());
                }
            }

            jObject[Constants.Announced] = announced;
            jObject[Constants.Deleted] = Deleted;

            JProperty jProperty = new JProperty(this.Name, jObject);

            return jProperty;
        }

        public JProperty ConvertToJPropertyWithoutSystemProperties()
        {
            JObject jObject = new JObject
            {
                [Constants.Mode] = (int)this.Mode
            };

            JArray announced = new JArray();
            JArray Deleted = new JArray();

            if (this.Announced != null)
            {
                foreach (var item in this.Announced)
                {
                    announced.Add(item.ConvertToJObjectWithoutSystemProperties());
                }
            }

            if (this.Deleted != null)
            {
                foreach (var item in this.Deleted)
                {
                    Deleted.Add(item.ConvertToJObjectWithoutSystemProperties());
                }
            }

            jObject[Constants.Announced] = announced;
            jObject[Constants.Deleted] = Deleted;

            JProperty jProperty = new JProperty(this.Name, jObject);

            return jProperty;
        }

        public static ESQLMultiItem ConvertFromJProperty(JProperty jProperty)
        {
            if (jProperty == null)
                return null;

            ESQLMultiItem multiFragment = new ESQLMultiItem
            {
                BlockInfo = new ESQLBlockDefinitionAttribute(jProperty[Constants.SystemPropertyTypeName] != null ? jProperty[Constants.SystemPropertyTypeName].Value<string>() : jProperty.Name),
                Name = jProperty.Name,
                Mode = (ModeForMultiItems)jProperty[Constants.Mode].Value<int>()
            };

            if (jProperty[Constants.Announced] == null)
            {
                multiFragment.Announced = new List<ESQLItem>();
            }
            else
            {
                multiFragment.Announced = (jProperty[Constants.Announced] as JArray).Children()
                    .Select(x => x as JObject).Select(x => ESQLItem.ConvertFromJObject(x));
            }

            if (jProperty[Constants.Deleted] == null)
            {
                multiFragment.Deleted = new List<ESQLItem>();
            }
            else
            {
                multiFragment.Deleted = (jProperty[Constants.Deleted] as JArray).Children()
                    .Select(x => x as JObject).Select(x => ESQLItem.ConvertFromJObject(x));
            }

            return multiFragment;
        }

        public static bool DeepEquals(ESQLMultiItem item1, ESQLMultiItem item2)
        {
            if (item1 == null || item2 == null)
                return false;

            var result =
                item1.Name == item2.Name
                && ESQLBlockDefinitionAttribute.DeepEquals(item1.BlockInfo, item2.BlockInfo)
                && item1.Mode == item2.Mode
                && ESQLItem.DeepEquals(item1.Announced, item2.Announced)
                && ESQLItem.DeepEquals(item1.Deleted, item2.Deleted);

            return result;
        }

        public static bool DeepEquals(IEnumerable<ESQLMultiItem> list1, IEnumerable<ESQLMultiItem> list2)
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

                if (!ESQLMultiItem.DeepEquals(item1, item2))
                    return false;
            }

            return true;
        }

        public ESQLMultiItem DeepClone()
        {
            return new ESQLMultiItem()
            {
                Mode = this.Mode,
                Name = this.Name,
                Announced = this.Announced?.Select(x => x.DeepClone()),
                Deleted = this.Deleted?.Select(x => x.DeepClone()),
                BlockInfo = this.BlockInfo.DeepClone()
            };
        }
    }
}