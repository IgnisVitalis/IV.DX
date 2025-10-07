using IV.DataProvider.Persistence.Contracts.Models;
using IV.DX.Contracts.Common.Attributes;
using IV.DX.Contracts.Common.Enums;

namespace IV.DX.Contracts.Common.Models
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