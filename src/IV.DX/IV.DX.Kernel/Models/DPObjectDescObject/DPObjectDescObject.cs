using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
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