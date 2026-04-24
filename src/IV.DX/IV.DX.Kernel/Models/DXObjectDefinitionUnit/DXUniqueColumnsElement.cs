using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXElement("DXUniqueColumnsElement")]
    public class DXUniqueColumnsElement : DXElement
    {
        [DXColumn("Columns")]
        public string Columns { get; set; } = null!;
    }
}