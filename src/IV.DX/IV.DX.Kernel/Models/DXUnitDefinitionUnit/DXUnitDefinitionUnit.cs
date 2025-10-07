using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters;

namespace IV.DX.Kernel.Models
{
    [ESQLObjectDefinition("DXUnitDefinitionUnit")]
    public class DXUnitDefinitionUnit : DXObjectDefinitionUnit
    {
        public static ESQLModelDefinition ESQLModelDefinition { get; } = ModelConverter.GetESQLModelDefinition<DXUnitDefinitionUnit>();

        public DXUnitInheritanceElement DXUnitInheritanceElement { get; set; }
        public ESQLMultiItemsContainer<DXElementInUnitDefinitionMainElement> DXElementInUnitDefinitionMainElement { get; set; }
    }
}