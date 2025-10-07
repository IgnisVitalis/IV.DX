using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;

namespace IV.DX.Kernel.Models
{
    [DXElement("DXUnitDefinitionMainElement")]
    public class DXUnitDefinitionMainElement : ESQLBlock
    {
        [DXColumn("Name")]
        public string Name { get; set; }
        [DXColumn("DisplayValue")]
        public string DisplayValue { get; set; }
        [DXColumn("Kind")]
        public DXObjectKindEnum Kind { get; set; }

        public DXUnitDefinitionMainElement()
        {
            this.Kind = DXObjectKindEnum.Custom;
        }
    }
}