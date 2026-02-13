using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXIdentityUnit")]
    public class DXIdentityUnit : DXUnit
    {
        [DXColumn("Name")]
        public string Name { get; set; }
    }
}
