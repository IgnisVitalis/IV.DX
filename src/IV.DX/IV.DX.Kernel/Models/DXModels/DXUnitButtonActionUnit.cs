using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXUnitButtonActionUnit")]
    public class DXUnitButtonActionUnit : DXUnitBaseActionUnit
    {
        [DXColumn("Name")]
        public string Name { get; set; } = null!;
    }
}
