using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [ESQLObjectDefinition("DPRelationItemObject")]
    public class DPRelationItemObject : ESQLObject
    {
        public DPRelationItemGenBlock DPRelationItemGenBlock { get; set; }
    }
}