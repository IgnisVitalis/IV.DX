using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Models
{
    public class DXItem
    {
        public Guid? ID { get; set; }
        public Guid? DXUnitID { get; set; }

        public JObject Content { get; set; }

        public bool HasValue(string propertyName)
        {
            var property = this.Content[propertyName];

            return property != null;
        }

        public T GetValue<T>(string propertyName)
        {
            var property = this.Content[propertyName];

            return property.Value<T>();
        }

        public JObject ConvertToJObject()
        {
            JObject jObject = this.Content != null ? new JObject(this.Content) : new JObject();

            jObject[Constants.ID] = this.ID;
            jObject[Constants.DXUnitID] = this.DXUnitID;

            return jObject;
        }

        public JObject ConvertToJObjectWithoutSystemProperties()
        {
            JObject obj = this.ConvertToJObject().DeepClone() as JObject;

            var systemProperties = obj.Properties().Where(x =>
                    x.Name.Length >= Constants.SystemPropertyPrefix.Length
                    && x.Name.Substring(0, Constants.SystemPropertyPrefix.Length) == Constants.SystemPropertyPrefix
                ).ToList();

            foreach (var systemProperty in systemProperties)
            {
                obj.Remove(systemProperty.Name);
            }

            return obj;
        }

        public static DXItem ConvertFromJObject(JObject jObject)
        {
            DXItem fragment = new DXItem
            {
                ID = jObject[Constants.ID] != null ? (Guid?)jObject[Constants.ID] : null,
                DXUnitID = jObject[Constants.DXUnitID] != null ? (Guid?)jObject[Constants.DXUnitID] : null
            };

            var jObjectCopy = jObject.DeepClone() as JObject;

            jObjectCopy.Remove(Constants.ID);
            jObjectCopy.Remove(Constants.DXUnitID);

            fragment.Content = jObjectCopy;

            return fragment;
        }

        public static DXItem Combine(params DXItem[] items)
        {
            DXItem result = new DXItem()
            {
                Content = new JObject()
            };

            foreach (var fragment in items.Where(x => x != null).ToList())
            {
                if (fragment.ID.HasValue)
                {
                    result.ID = fragment.ID;
                }

                JObject jObject = fragment.Content;

                if (jObject == null)
                    continue;

                // Copy properties
                foreach (var item in jObject.Properties().Where(x => x.Value is JValue).ToList())
                {
                    if (result.Content.ContainsKey(item.Name))
                    {
                        result.Content[item.Name] = item.Value;
                    }
                    else
                    {
                        result.Content.Add(item.DeepClone());
                    }
                }

                // Copy relations
                foreach (var item in jObject.Properties().Where(x => x.Value is JObject).ToList())
                {
                    if (!result.Content.ContainsKey(item.Name))
                    {
                        JObject jObjectForRel = new JObject
                        {
                            { Constants.Announced, new JArray() },
                            { Constants.Deleted, new JArray() }
                        };

                        result.Content.Add(item.Name, jObjectForRel);
                    }

                    var addedRelations = item.Value[Constants.Announced];

                    if (addedRelations != null && addedRelations is JArray)
                    {
                        var idsFromIncomeObj = (item.Value[Constants.Announced] as JArray).ToObject<IEnumerable<Guid>>();
                        var idsFromCurrentObj = (result.Content[item.Name][Constants.Announced] as JArray).ToObject<IEnumerable<Guid>>();

                        result.Content[item.Name][Constants.Announced] = new JArray(idsFromIncomeObj.Concat(idsFromCurrentObj).ToList());
                    }

                    var removedRelations = item.Value[Constants.Deleted];

                    if (removedRelations != null && removedRelations is JArray)
                    {
                        var idsFromIncomeObj = (item.Value[Constants.Deleted] as JArray).ToObject<IEnumerable<Guid>>();
                        var idsFromCurrentObj = (result.Content[item.Name][Constants.Deleted] as JArray).ToObject<IEnumerable<Guid>>();

                        result.Content[item.Name][Constants.Deleted] = new JArray(idsFromIncomeObj.Concat(idsFromCurrentObj).ToList());
                    }
                }
            }

            return result;
        }

        public static bool DeepEquals(DXItem item1, DXItem item2)
        {
            if (item1 == null || item2 == null)
                return false;

            var result = true;

            if (item1.ID.HasValue && item2.ID.HasValue)
            {
                result = result && (item1.ID.Value == item2.ID.Value);
            }
            else
            {
                result = result && (item1.ID == item2.ID);
            }

            if (item1.DXUnitID.HasValue && item2.DXUnitID.HasValue)
            {
                result = result && (item1.DXUnitID.Value == item2.DXUnitID.Value);
            }
            else
            {
                result = result && (item1.DXUnitID == item2.DXUnitID);
            }

            result = result && JToken.DeepEquals(item1.Content, item2.Content);

            return result;
        }

        public static bool DeepEquals(IEnumerable<DXItem> list1, IEnumerable<DXItem> list2)
        {
            if (list1 == null || list2 == null)
                return true;

            if (list1.Count() != list2.Count())
                return false;

            foreach (var item1 in list1)
            {
                if (!item1.ID.HasValue)
                    return false;

                var item2 = list2.SingleOrDefault(x => x.ID.HasValue && x.ID.Value == item1.ID.Value);

                if (item2 == null)
                    return false;

                if (!DXItem.DeepEquals(item1, item2))
                    return false;
            }

            return true;
        }

        public DXItem DeepClone()
        {
            return new DXItem()
            {
                ID = this.ID,
                DXUnitID = this.DXUnitID,
                Content = this.Content?.DeepClone() as JObject
            };
        }
    }
}