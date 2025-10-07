using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters;

namespace IV.DX.Kernel.Models
{
    [ESQLObjectDefinition("DXUnitDefinitionUnit")]
    public class DXUnitDefinitionUnit : DPObjectDescObject
    {
        public static ESQLModelDefinition ESQLModelDefinition { get; } = ModelConverter.GetESQLModelDefinition<DXUnitDefinitionUnit>();

        public DPEntityInheritanceBlock DPEntityInheritanceBlock { get; set; }
        public ESQLMultiItemsContainer<DPBlockInEntityDescGenBlock> DPBlockInEntityDescGenBlock { get; set; }
    }
}