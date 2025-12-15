using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXUnitDefinitionUnit")]
    public class DXUnitDefinitionUnit : DXObjectDefinitionUnit
    {
        [DXColumn("BaseDXUnit")]
        public Guid? BaseDXUnit { get; set; }
        public DXMultiElementsContainer<DXElementInUnitDefinitionElement> DXElementInUnitDefinitionElement { get; set; }
        public DXMultiElementsContainer<DXUnitRelationElement> DXUnitRelationElement { get; set; }

        public DXUnitDefinitionUnit()
        {
            this.DXElementInUnitDefinitionElement = new DXMultiElementsContainer<DXElementInUnitDefinitionElement>
            {
                Announced = new HashSet<DXElementInUnitDefinitionElement>()
            };

            this.DXUnitRelationElement = new DXMultiElementsContainer<DXUnitRelationElement>
            {
                Announced = new HashSet<DXUnitRelationElement>()
            };
        }
    }
}