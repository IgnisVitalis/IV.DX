using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXInheritanceInitCore")]
    public class DXInheritanceInitCore : DXUnit
    {
        public string BaseDXUnit { get; set; }
        public string ChildDXUnit { get; set; }
    }
}