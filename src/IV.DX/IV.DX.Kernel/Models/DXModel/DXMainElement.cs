using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    public class DXMainElement
    {
        public DXUnitAttribute Attribute { get; private set; }
        public DXItem Item { get; set; }

        public DXMainElement(DXUnitAttribute attribute)
        {
            this.Attribute = attribute;
        }

        public static bool DeepEquals(DXMainElement item1, DXMainElement item2)
        {
            if (item1 == null || item2 == null)
                return false;

            var result =
                DXUnitAttribute.DeepEquals(item1.Attribute, item2.Attribute)
                && DXItem.DeepEquals(item1.Item, item2.Item);

            return result;
        }

        public DXMainElement DeepClone()
        {
            return new DXMainElement(this.Attribute.DeepClone())
            {
                Item = this.Item.DeepClone(),
            };
        }
    }
}