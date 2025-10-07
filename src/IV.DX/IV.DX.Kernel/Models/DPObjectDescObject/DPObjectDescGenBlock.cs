using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;

namespace IV.DX.Kernel.Models
{
    [ESQLBlockDefinition("DXUnitDefinitionMainElement")]
    public class DXUnitDefinitionMainElement : ESQLBlock
    {
        [ESQLColumnDefinition("Name")]
        public string Name { get; set; }
        [ESQLColumnDefinition("DisplayValue")]
        public string DisplayValue { get; set; }
        [ESQLColumnDefinition("Kind")]
        public DXObjectKindEnum Kind { get; set; }

        public DXUnitDefinitionMainElement()
        {
            this.Kind = DXObjectKindEnum.Custom;
        }
    }
}