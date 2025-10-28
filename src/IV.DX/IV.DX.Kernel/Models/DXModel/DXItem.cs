using IV.DX.Kernel.Helpers;
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

            result = result && JHelper.DeepEquals(item1.Content, item2.Content);

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