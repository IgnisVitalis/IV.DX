using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXInheritanceInitCore")]
    public class DXInheritanceInitCore : DXUnit
    {
        [DXColumn("BaseDXUnit")]
        public string BaseDXUnit { get; set; }
        [DXColumn("ChildDXUnit")]
        public string ChildDXUnit { get; set; }
    }
}
