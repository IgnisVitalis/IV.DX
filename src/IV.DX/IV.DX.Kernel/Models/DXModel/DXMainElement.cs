using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    public class DXMainElement
    {
        public DXUnitAttribute Attribute { get; }
        public DXItem Item { get; }

        public DXMainElement(DXUnitAttribute attribute, DXItem item)
        {
            this.Item = item;
            this.Attribute = attribute;
        }

        public bool DeepEquals(DXMainElement item2)
        {
            if (item2 == null)
                return false;

            var result =
                DXUnitAttribute.DeepEquals(this.Attribute, item2.Attribute)
                && this.Item.DeepEquals(item2.Item);

            return result;
        }

        public DXMainElement DeepClone()
        {
            return new DXMainElement(this.Attribute.DeepClone(), this.Item.DeepClone());
        }
    }
}