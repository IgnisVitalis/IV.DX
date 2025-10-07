using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [ESQLBlockDefinition("DXUniqueColumnsElement")]
    public class DXUniqueColumnsElement : ESQLBlock
    {
        [ESQLColumnDefinition("Columns")]
        public string Columns { get; set; }
    }
}