using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXElement("DXUniqueColumnsElement")]
    public class DXUniqueColumnsElement : ESQLBlock
    {
        [DXColumn("Columns")]
        public string Columns { get; set; }
    }
}