using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXElementDefinitionUnit")]
    public class DXElementDefinitionUnit : DXObjectDefinitionUnit
    {
        [DXColumn("IsCommon")]
        public bool IsCommon { get; set; }

        public DXMultiElementsContainer<DXElementToUnitRelationElement> DXElementToUnitRelationElement { get; set; } = null!;
    }
}
