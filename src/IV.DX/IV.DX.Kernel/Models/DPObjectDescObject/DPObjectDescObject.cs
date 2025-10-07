using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXObjectDefinitionUnit")]
    public class DXObjectDefinitionUnit : ESQLObject
    {
        public DXUnitDefinitionMainElement DXUnitDefinitionMainElement { get; set; }

        public ESQLMultiItemsContainer<DXColumnDefinitionElement> DXColumnDefinitionElement { get; set; }
        public ESQLMultiItemsContainer<DXUniqueColumnsElement> DXUniqueColumnsElement { get; set; }

        public DXObjectDefinitionUnit()
        {
            this.DXColumnDefinitionElement = new ESQLMultiItemsContainer<DXColumnDefinitionElement>
            {
                Announced = new List<DXColumnDefinitionElement>()
            };

            this.DXUniqueColumnsElement = new ESQLMultiItemsContainer<DXUniqueColumnsElement>
            {
                Announced = new List<DXUniqueColumnsElement>()
            };
        }
    }
}