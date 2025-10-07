using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;

namespace IV.DX.Kernel.Models
{
    [ESQLBlockDefinition("DXElementInUnitDefinitionMainElement")]
    public class DXElementInUnitDefinitionMainElement : ESQLBlock
    {
        [ESQLColumnDefinition("RelationType")]
        public DXElementInUnitTypeEnum RelationType { get; set; }

        [ESQLColumnDefinition("DXElementDefinitionUnit")]
        public Guid DXElementDefinitionUnit { get; set; }
    }
}