using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters.DXModelConverters;
using IV.DX.Kernel.Converters.JObjectConverters;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Models
{
    public class DXSingleElement
    {
        public string Name { get; set; }
        public DXElementAttribute Attribute { get; set; }
        public DXItem Item { get; set; }

     

        public static bool DeepEquals(DXSingleElement item1, DXSingleElement item2)
        {
            if (item1 == null || item2 == null)
                return false;

            var result =
                item1.Name == item2.Name
                && DXElementAttribute.DeepEquals(item1.Attribute, item2.Attribute)
                && DXItem.DeepEquals(item1.Item, item2.Item);

            return result;
        }

        public static bool DeepEquals(IEnumerable<DXSingleElement> list1, IEnumerable<DXSingleElement> list2)
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

                if (!DXSingleElement.DeepEquals(item1, item2))
                    return false;
            }

            return true;
        }

        public DXSingleElement DeepClone()
        {
            return new DXSingleElement()
            {
                Attribute = this.Attribute?.DeepClone(),
                Name = this.Name,
                Item = this.Item.DeepClone()
            };
        }
    }
}