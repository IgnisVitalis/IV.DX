using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXUnitDefinitionUnit")]
    public class DXUnitDefinitionUnit : DXObjectDefinitionUnit
    {
        [DXColumn("BaseDXUnit")]
        public Guid? BaseDXUnit { get; set; }
 
        public DXMultiElementsContainer<DXElementInUnitDefinitionElement> DXElementInUnitDefinitionElement { get; set; }

        public DXMultiElementsContainer<DXUnitToUnitRelationElement> DXUnitToUnitRelationElement { get; set; }

        public DXMultiElementsContainer<DXUnitToElementRelationElement> DXUnitToElementRelationElement { get; set; }

        public DXUnitDefinitionUnit()
        {
            this.DXElementInUnitDefinitionElement = new DXMultiElementsContainer<DXElementInUnitDefinitionElement>
            {
                Announced = new HashSet<DXElementInUnitDefinitionElement>()
            };

            this.DXUnitToUnitRelationElement = new DXMultiElementsContainer<DXUnitToUnitRelationElement>
            {
                Announced = new HashSet<DXUnitToUnitRelationElement>()
            };

            this.DXUnitToElementRelationElement = new DXMultiElementsContainer<DXUnitToElementRelationElement>
            {
                Announced = new HashSet<DXUnitToElementRelationElement>()
            };
        }
    }
}
