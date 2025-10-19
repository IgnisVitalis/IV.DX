using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXUnitDefinitionUnit")]
    public class DXUnitDefinitionUnit : DXObjectDefinitionUnit
    {
        public DXUnitInheritanceElement DXUnitInheritanceElement { get; set; }
        public DXMultiElementsContainer<DXElementInUnitDefinitionElement> DXElementInUnitDefinitionElement { get; set; }
    }
}