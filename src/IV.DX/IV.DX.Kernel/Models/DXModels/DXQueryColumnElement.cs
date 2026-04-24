using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXElement("DXQueryColumnElement")]
    public class DXQueryColumnElement : DXElement
    {
        [DXColumn("Name")]
        public string Name { get; set; } = null!;

        [DXColumn("Expression")]
        public string Expression { get; set; } = null!;

        [DXColumn("Order")]
        public int Order { get; set; }
    }
}
