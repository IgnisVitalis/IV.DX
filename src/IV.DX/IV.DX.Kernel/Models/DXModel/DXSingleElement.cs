using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    public class DXSingleElement
    {
        public string Name { get; }
        public DXElementAttribute Attribute { get; }
        public DXItem? Item { get; }

        public DXSingleElement(string name, DXElementAttribute attribute)
        {
            this.Name = name;
            this.Attribute = attribute;
        }

        public DXSingleElement(string name, DXElementAttribute attribute, DXItem item) : this(name, attribute)
        {
            this.Item = item;
        }

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

                if (!DeepEquals(item1, item2))
                    return false;
            }

            return true;
        }

        public DXSingleElement DeepClone()
        {
            return new DXSingleElement(this.Name, this.Attribute.DeepClone(), this.Item.DeepClone());
        }
    }
}