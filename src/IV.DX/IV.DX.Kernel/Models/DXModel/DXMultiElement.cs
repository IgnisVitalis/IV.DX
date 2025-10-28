using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    public class DXMultiElement
    {
        public string Name { get; set; }
        public DXElementAttribute Attribute { get; set; }
        public MultiElementsMode Mode { get; set; } = MultiElementsMode.Full;
        public HashSet<DXItem> Announced { get; set; } = new HashSet<DXItem>();
        public HashSet<DXItem> Deleted { get; set; } = new HashSet<DXItem>();

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
            return new DXMultiElement()
            {
                Mode = this.Mode,
                Name = this.Name,
                Announced = this.Announced?.Select(x => x.DeepClone()).ToHashSet(),
                Deleted = this.Deleted?.Select(x => x.DeepClone()).ToHashSet(),
                Attribute = this.Attribute.DeepClone()
            };
        }
    }
}