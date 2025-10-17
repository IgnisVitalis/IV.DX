using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXObjectDefinitionUnit")]
    public class DXObjectDefinitionUnit : DXUnit
    {
        public DXUnitDefinitionMainElement DXUnitDefinitionMainElement { get; set; }

        public DXMultiElementsContainer<DXColumnDefinitionElement> DXColumnDefinitionElement { get; set; }
        public DXMultiElementsContainer<DXUniqueColumnsElement> DXUniqueColumnsElement { get; set; }

        public DXObjectDefinitionUnit()
        {
            this.DXColumnDefinitionElement = new DXMultiElementsContainer<DXColumnDefinitionElement>
            {
                Announced = new List<DXColumnDefinitionElement>()
            };

            this.DXUniqueColumnsElement = new DXMultiElementsContainer<DXUniqueColumnsElement>
            {
                Announced = new List<DXUniqueColumnsElement>()
            };
        }
    }
}