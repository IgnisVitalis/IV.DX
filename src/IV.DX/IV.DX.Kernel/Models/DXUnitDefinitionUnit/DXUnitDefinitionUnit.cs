using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXUnitDefinitionUnit")]
    public class DXUnitDefinitionUnit : DXObjectDefinitionUnit
    {
        public static ESQLModelDefinition ESQLModelDefinition { get; } = DXModelConverter.GetESQLModelDefinition<DXUnitDefinitionUnit>();

        public DXUnitInheritanceElement DXUnitInheritanceElement { get; set; }
        public ESQLMultiItemsContainer<DXElementInUnitDefinitionMainElement> DXElementInUnitDefinitionMainElement { get; set; }
    }
}