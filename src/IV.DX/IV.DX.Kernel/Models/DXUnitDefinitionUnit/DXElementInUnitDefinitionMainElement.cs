using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;

namespace IV.DX.Kernel.Models
{
    [DXElement("DXElementInUnitDefinitionMainElement")]
    public class DXElementInUnitDefinitionMainElement : DXElement
    {
        [DXColumn("RelationType")]
        public DXElementInUnitTypeEnum RelationType { get; set; }

        [DXColumn("DXElementDefinitionUnit")]
        public Guid DXElementDefinitionUnit { get; set; }
    }
}