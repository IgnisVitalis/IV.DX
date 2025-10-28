namespace IV.DX.Kernel.Models
{
    public class DXModel
    {
        public DXMainElement DXMainElement { get; set; }
        public HashSet<DXSingleElement> DXSingleElements { get; set; }
        public HashSet<DXMultiElement> DXMultiElements { get; set; }

        public DXModel(DXMainElement mainElement)
        {
            this.DXMainElement = mainElement;
        }

        public static bool DeepEquals(DXModel item1, DXModel item2)
        {
            if (item1 == null || item2 == null)
                return false;

            var result = true;

            result = result
                && DXMainElement.DeepEquals(item1.DXMainElement, item2.DXMainElement)
                && DXSingleElement.DeepEquals(item1.DXSingleElements, item2.DXSingleElements)
                && DXMultiElement.DeepEquals(item1.DXMultiElements, item2.DXMultiElements);

            return result;
        }

        public DXModel DeepClone()
        {
            var ownItemClone = this.DXMainElement.DeepClone();

            return new DXModel(ownItemClone)
            {
                DXSingleElements = this.DXSingleElements?.Select(x => x.DeepClone()).ToHashSet(),
                DXMultiElements = this.DXMultiElements?.Select(x => x.DeepClone()).ToHashSet()
            };
        }
    }
}