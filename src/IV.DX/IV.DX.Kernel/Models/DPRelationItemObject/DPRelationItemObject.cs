using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DPRelationItemObject")]
    public class DPRelationItemObject : ESQLObject
    {
        public DPRelationItemGenBlock DPRelationItemGenBlock { get; set; }
    }
}