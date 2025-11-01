using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    public class DXSingleElement
    {
        public string Name { get; }
        public DXElementAttribute Attribute { get; }
        public DXItem Item { get; }
        public bool IsRequired { get; }

        public DXSingleElement(string name, DXElementAttribute attribute, DXItem item, bool isRequired)
        {
            this.Item = item;
            this.IsRequired = isRequired;
            this.Name = name;
            this.Attribute = attribute;
        }

        public bool DeepEquals(DXSingleElement item2)
        {
            if (item2 == null)
                return false;

            var result =
                this.Name == item2.Name
                && DXElementAttribute.DeepEquals(this.Attribute, item2.Attribute)
                && this.Item.DeepEquals(item2.Item);

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

                if (!item1.DeepEquals(item2))
                    return false;
            }

            return true;
        }

        public DXSingleElement DeepClone()
        {
            return new DXSingleElement(this.Name, this.Attribute.DeepClone(), this.Item.DeepClone(), this.IsRequired);
        }
    }
}