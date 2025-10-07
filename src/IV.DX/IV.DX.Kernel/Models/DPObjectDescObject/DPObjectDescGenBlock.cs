using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;

namespace IV.DX.Kernel.Models
{
    [ESQLBlockDefinition("DPObjectDescGenBlock")]
    public class DPObjectDescGenBlock : ESQLBlock
    {
        [ESQLColumnDefinition("Name")]
        public string Name { get; set; }
        [ESQLColumnDefinition("DisplayValue")]
        public string DisplayValue { get; set; }
        [ESQLColumnDefinition("Kind")]
        public DPObjectKindEnum Kind { get; set; }

        public DPObjectDescGenBlock()
        {
            this.Kind = DPObjectKindEnum.Custom;
        }
    }
}