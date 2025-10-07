using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [ESQLObjectDefinition("DPObjectDescObject")]
    public class DPObjectDescObject : ESQLObject
    {
        public DPObjectDescGenBlock DPObjectDescGenBlock { get; set; }

        public ESQLMultiItemsContainer<DXColumnDefinitionElement> DXColumnDefinitionElement { get; set; }
        public ESQLMultiItemsContainer<DXUniqueColumnsElement> DXUniqueColumnsElement { get; set; }

        public DPObjectDescObject()
        {
            this.DXColumnDefinitionElement = new ESQLMultiItemsContainer<DXColumnDefinitionElement>
            {
                Announced = new List<DXColumnDefinitionElement>()
            };

            this.DXUniqueColumnsElement = new ESQLMultiItemsContainer<DXUniqueColumnsElement>
            {
                Announced = new List<DXUniqueColumnsElement>()
            };
        }
    }
}