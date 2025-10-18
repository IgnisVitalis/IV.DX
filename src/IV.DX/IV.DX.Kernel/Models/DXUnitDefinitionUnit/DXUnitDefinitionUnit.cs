using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXUnitDefinitionUnit")]
    public class DXUnitDefinitionUnit : DXObjectDefinitionUnit
    {
        public static DXModelDefinition DXModelDefinition { get; } = DXModelDefinitionHelper.GetDXModelDefinition<DXUnitDefinitionUnit>();

        public DXUnitInheritanceElement DXUnitInheritanceElement { get; set; }
        public DXMultiElementsContainer<DXElementInUnitDefinitionElement> DXElementInUnitDefinitionElement { get; set; }
    }
}