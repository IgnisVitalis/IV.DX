using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;

namespace IV.DX.Kernel.Models
{
    [DXElement("DXObjectDefinitionMainElement")]
    public class DXObjectDefinitionMainElement : DXElement
    {
        [DXColumn("Name")]
        public string Name { get; set; }
        [DXColumn("DisplayValue")]
        public string DisplayValue { get; set; }
        [DXColumn("Kind")]
        public DXObjectKindEnum Kind { get; set; }

        public DXObjectDefinitionMainElement()
        {
            this.Kind = DXObjectKindEnum.Custom;
        }
    }
}