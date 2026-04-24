using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXButtonActionUnit")]
    public class DXButtonActionUnit : DXBaseActionUnit
    {
        [DXColumn("Name")]
        public string Name { get; set; } = null!;
    }
}
