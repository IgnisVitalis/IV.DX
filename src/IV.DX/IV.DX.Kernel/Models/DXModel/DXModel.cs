using IV.DX.Kernel.Converters.DXModelConverters;
using IV.DX.Kernel.Converters.JObjectConverters;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Models
{
    public class DXModel
    {
        public DXMainElement DXMainElement { get; }
        public HashSet<DXSingleElement> DXSingleElements { get; }
        public HashSet<DXMultiElement> DXMultiElements { get; }

        public DXModel(DXMainElement mainElement, HashSet<DXSingleElement> dxSingleElements, HashSet<DXMultiElement> dxMultiElements)
        {
            this.DXMainElement = mainElement;
            this.DXSingleElements = dxSingleElements;
            this.DXMultiElements = dxMultiElements;
        }

        public JObject ToJObject()
        {
            return JObjectConverter.ToJObject(this);
        }

        public static DXModel From(JObject jObject)
        {
            return DXModelConverter.ToDXModel(jObject);
        }

        public void AddSingleElement(DXSingleElement singleElement)
        {
            this.DXSingleElements.Add(singleElement);
        }

        public void RemoveSingleElement(DXSingleElement singleElement)
        {
            this.DXSingleElements.Remove(singleElement);
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

            var dxSingleElements = this.DXSingleElements?.Select(x => x.DeepClone()).ToHashSet();
            var dxMultiElements = this.DXMultiElements?.Select(x => x.DeepClone()).ToHashSet();

            return new DXModel(ownItemClone, dxSingleElements, dxMultiElements);
        }
    }
}