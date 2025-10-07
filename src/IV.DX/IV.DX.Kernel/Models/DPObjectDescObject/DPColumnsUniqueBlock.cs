using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [ESQLBlockDefinition("DPColumnsUniqueBlock")]
    public class DPColumnsUniqueBlock : ESQLBlock
    {
        [ESQLColumnDefinition("Columns")]
        public string Columns { get; set; }
    }
}