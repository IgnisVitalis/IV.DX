using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXObjectDefinitionUnit")]
    public class DXObjectDefinitionUnit : DXUnit
    {
        [DXColumn("Name")]
        public string Name { get; set; }
        [DXColumn("DisplayValue")]
        public string DisplayValue { get; set; }
        [DXColumn("Kind")]
        public DXObjectKindEnum Kind { get; set; }
             

        public DXMultiElementsContainer<DXColumnDefinitionElement> DXColumnDefinitionElement { get; set; }

        public DXMultiElementsContainer<DXUniqueColumnsElement> DXUniqueColumnsElement { get; set; }

        public DXMultiElementsContainer<DXObjectEnumElement> DXObjectEnumElement { get; set; }

        public DXObjectDefinitionUnit()
        {
            this.Kind = DXObjectKindEnum.Custom;

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
    }
}