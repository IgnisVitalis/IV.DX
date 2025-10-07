using IV.DataProvider.Persistence.Contracts.Models;
using IV.DX.Contracts.Common.Attributes;
using IV.DX.Contracts.Common.Converters;

namespace IV.DX.Contracts.Common.Models
{
    [ESQLObjectDefinition("DPEntityDescObject")]
    public class DPEntityDescObject : DPObjectDescObject
    {
        public static ESQLModelDefinition ESQLModelDefinition { get; } = ModelConverter.GetESQLModelDefinition<DPEntityDescObject>();

        public DPEntityInheritanceBlock DPEntityInheritanceBlock { get; set; }
        public ESQLMultiItemsContainer<DPBlockInEntityDescGenBlock> DPBlockInEntityDescGenBlock { get; set; }
    }
}