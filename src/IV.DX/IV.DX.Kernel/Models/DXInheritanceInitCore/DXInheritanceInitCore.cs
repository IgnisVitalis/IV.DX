using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXInheritanceInitCore")]
    public class DXInheritanceInitCore : DXUnit
    {
        public string BaseEntity { get; set; }
        public string ChildEntity { get; set; }
    }
}