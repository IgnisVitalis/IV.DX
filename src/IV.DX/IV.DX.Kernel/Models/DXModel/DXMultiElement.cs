using IV.DX.Kernel.Attributes;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Models
{
    public class DXMultiElement
    {
        public string Name { get; set; }
        public DXElementAttribute DXElementInfo { get; set; }
        public MultiElementsMode Mode { get; set; } = MultiElementsMode.Full;
        public HashSet<DXItem> Announced { get; set; } = new HashSet<DXItem>();
        public HashSet<DXItem> Deleted { get; set; } = new HashSet<DXItem>();

        public JProperty ConvertToJProperty()
        {
            JObject jObject = new JObject
            {
                [Constants.SystemPropertyTypeName] = this.DXElementInfo.Name,
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

        public void AddToAnnounced(DXItem dxItem)
        {
            this.Announced.Add(dxItem);
        }

        public void RemoveFromAnnounced(DXItem dxItem)
        {
            this.Announced.Remove(dxItem);
        }

        public void AddToDeleted(DXItem dxItem)
        {
            this.Deleted.Add(dxItem);
        }

        public void RemoveFromDeleted(DXItem dxItem)
        {
            this.Deleted.Remove(dxItem);
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

        public static DXMultiElement ConvertFromJProperty(JProperty jProperty)
        {
            if (jProperty == null)
                return null;

            DXMultiElement multiFragment = new DXMultiElement
            {
                DXElementInfo = new DXElementAttribute(jProperty[Constants.SystemPropertyTypeName] != null ? jProperty[Constants.SystemPropertyTypeName].Value<string>() : jProperty.Name),
                Name = jProperty.Name,
                Mode = (MultiElementsMode)jProperty[Constants.Mode].Value<int>()
            };

            if (jProperty[Constants.Announced] == null)
            {
                multiFragment.Announced = new HashSet<DXItem>();
            }
            else
            {
                multiFragment.Announced = (jProperty[Constants.Announced] as JArray).Children()
                    .Select(x => x as JObject).Select(x => DXItem.ConvertFromJObject(x)).ToHashSet();
            }

            if (jProperty[Constants.Deleted] == null)
            {
                multiFragment.Deleted = new HashSet<DXItem>();
            }
            else
            {
                multiFragment.Deleted = (jProperty[Constants.Deleted] as JArray).Children()
                    .Select(x => x as JObject).Select(x => DXItem.ConvertFromJObject(x)).ToHashSet();
            }

            return multiFragment;
        }

        public static bool DeepEquals(DXMultiElement item1, DXMultiElement item2)
        {
            if (item1 == null || item2 == null)
                return false;

            var result =
                item1.Name == item2.Name
                && DXElementAttribute.DeepEquals(item1.DXElementInfo, item2.DXElementInfo)
                && item1.Mode == item2.Mode
                && DXItem.DeepEquals(item1.Announced, item2.Announced)
                && DXItem.DeepEquals(item1.Deleted, item2.Deleted);

            return result;
        }

        public static bool DeepEquals(IEnumerable<DXMultiElement> list1, IEnumerable<DXMultiElement> list2)
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

                if (!DXMultiElement.DeepEquals(item1, item2))
                    return false;
            }

            return true;
        }

        public DXMultiElement DeepClone()
        {
            return new DXMultiElement()
            {
                Mode = this.Mode,
                Name = this.Name,
                Announced = this.Announced?.Select(x => x.DeepClone()).ToHashSet(),
                Deleted = this.Deleted?.Select(x => x.DeepClone()).ToHashSet(),
                DXElementInfo = this.DXElementInfo.DeepClone()
            };
        }
    }
}