using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXRelationItemUnit")]
    public class DXRelationItemUnit : DXUnit
    {
        [DXRequired]
        public DXRelationItemMainElement DXRelationItemMainElement { get; set; }
    }
}