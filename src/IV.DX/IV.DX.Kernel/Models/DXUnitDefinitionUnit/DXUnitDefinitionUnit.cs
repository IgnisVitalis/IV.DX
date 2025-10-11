using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXUnitDefinitionUnit")]
    public class DXUnitDefinitionUnit : DXObjectDefinitionUnit
    {
        public static DXModelDefinition ESQLModelDefinition { get; } = DXModelDefinitionHelper.GetESQLModelDefinition<DXUnitDefinitionUnit>();

        public DXUnitInheritanceElement DXUnitInheritanceElement { get; set; }
        public DXMultiElementsContainer<DXElementInUnitDefinitionMainElement> DXElementInUnitDefinitionMainElement { get; set; }
    }
}