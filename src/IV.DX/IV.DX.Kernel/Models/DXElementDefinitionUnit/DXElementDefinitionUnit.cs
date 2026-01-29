using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXElementDefinitionUnit")]
    public class DXElementDefinitionUnit : DXObjectDefinitionUnit
    {
        public DXMultiElementsContainer<DXElementToUnitRelationElement> DXElementToUnitRelationElement { get; set; }
    }
}
