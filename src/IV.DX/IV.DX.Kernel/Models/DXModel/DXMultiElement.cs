using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    public class DXMultiElement
    {
        public string Name { get;  }
        public DXElementAttribute Attribute { get; }
        public MultiElementsMode Mode { get;  }
        public HashSet<DXItem> Announced { get; }
        public HashSet<DXItem> Deleted { get; }

        public bool IsRequired { get; }

        public DXMultiElement(
            string name, 
            DXElementAttribute attribute,
            MultiElementsMode mode,
            HashSet<DXItem> announced,
            HashSet<DXItem> deleted,
            bool isRequired)
        {
            this.Name = name;
            this.Attribute = attribute;
            this.Mode = mode;
            this.Announced = announced;
            this.Deleted = deleted;
            this.IsRequired = isRequired;
        }

        public static bool DeepEquals(DXMultiElement item1, DXMultiElement item2)
        {
            if (item1 == null || item2 == null)
                return false;

            var result =
                item1.Name == item2.Name
                && DXElementAttribute.DeepEquals(item1.Attribute, item2.Attribute)
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
            return new DXMultiElement(this.Name, this.Attribute.DeepClone(), this.Mode, this.Announced, this.Deleted, this.IsRequired);
        }
    }
}