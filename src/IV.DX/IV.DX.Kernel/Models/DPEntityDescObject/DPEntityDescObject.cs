using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters;

namespace IV.DX.Kernel.Models
{
    [ESQLObjectDefinition("DPEntityDescObject")]
    public class DPEntityDescObject : DPObjectDescObject
    {
        public static ESQLModelDefinition ESQLModelDefinition { get; } = ModelConverter.GetESQLModelDefinition<DPEntityDescObject>();

        public DPEntityInheritanceBlock DPEntityInheritanceBlock { get; set; }
        public ESQLMultiItemsContainer<DPBlockInEntityDescGenBlock> DPBlockInEntityDescGenBlock { get; set; }
    }
}