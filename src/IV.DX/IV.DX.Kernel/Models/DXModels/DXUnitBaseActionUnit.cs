using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXUnitBaseActionUnit")]
    public class DXUnitBaseActionUnit : DXBaseActionUnit
    {
        [DXColumn("UnitType")]
        public Guid UnitType { get; set; }
    }
}
