using IV.DataProvider.Persistence.Contracts.Models;
using IV.DX.Contracts.Common.Attributes;

namespace IV.DX.Contracts.Common.Models
{
    [ESQLObjectDefinition("DPObjectDescObject")]
    public class DPObjectDescObject : ESQLObject
    {
        public DPObjectDescGenBlock DPObjectDescGenBlock { get; set; }

        public ESQLMultiItemsContainer<DPColumnDescBlock> DPColumnDescBlock { get; set; }
        public ESQLMultiItemsContainer<DPColumnsUniqueBlock> DPColumnsUniqueBlock { get; set; }

        public DPObjectDescObject()
        {
            this.DPColumnDescBlock = new ESQLMultiItemsContainer<DPColumnDescBlock>
            {
                Announced = new List<DPColumnDescBlock>()
            };

            this.DPColumnsUniqueBlock = new ESQLMultiItemsContainer<DPColumnsUniqueBlock>
            {
                Announced = new List<DPColumnsUniqueBlock>()
            };
        }
    }
}