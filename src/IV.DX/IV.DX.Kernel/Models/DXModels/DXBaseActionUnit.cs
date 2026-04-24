using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXBaseActionUnit")]
    public class DXBaseActionUnit : DXUnit
    {
        [DXColumn("ActionDefinition")]
        public Guid ActionDefinition { get; set; }
    }
}