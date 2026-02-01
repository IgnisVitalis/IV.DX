using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXInheritanceInitCore")]
    internal class DXInheritanceInitCore : DXUnit
    {
        [DXColumn("BaseDXUnit")]
        public string BaseDXUnit { get; set; }
        [DXColumn("ChildDXUnit")]
        public string ChildDXUnit { get; set; }
    }
}
