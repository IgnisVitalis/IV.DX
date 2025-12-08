using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXObjectDefinitionUnit")]
    public class DXObjectDefinitionUnit : DXUnit
    {
        [DXRequired]
        public DXObjectDefinitionMainElement DXObjectDefinitionMainElement { get; set; }

        public DXMultiElementsContainer<DXColumnDefinitionElement> DXColumnDefinitionElement { get; set; }

        public DXMultiElementsContainer<DXUniqueColumnsElement> DXUniqueColumnsElement { get; set; }

        public DXMultiElementsContainer<DXObjectEnumElement> DXObjectEnumElement { get; set; }

        public DXObjectDefinitionUnit()
        {
            this.DXColumnDefinitionElement = new DXMultiElementsContainer<DXColumnDefinitionElement>
            {
                Announced = new HashSet<DXColumnDefinitionElement>()
            };

            this.DXUniqueColumnsElement = new DXMultiElementsContainer<DXUniqueColumnsElement>
            {
                Announced = new HashSet<DXUniqueColumnsElement>()
            };

            this.DXObjectEnumElement = new DXMultiElementsContainer<DXObjectEnumElement>
            {
                Announced = new HashSet<DXObjectEnumElement>()
            };
        }

        public IDictionary<string, string> GetColumns()
        {
            if (DXColumnDefinitionElement == null || DXColumnDefinitionElement.Announced == null || DXColumnDefinitionElement.Announced.Count() == 0)
                return new Dictionary<string, string>();

            return this.DXColumnDefinitionElement.Announced.ToDictionary(x => x.Name, x => x.Name);
        }
    }
}