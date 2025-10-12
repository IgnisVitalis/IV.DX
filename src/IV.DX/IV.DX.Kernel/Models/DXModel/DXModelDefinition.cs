using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    public class DXModelDefinition
    {
        public DXElementDefinition OwnSingleItem { get; private set; }
        public IEnumerable<DXElementDefinition> SingleFragmentDefinitions { get; private set; }
        public IEnumerable<DXElementDefinition> MultiFragmentDefinitions { get; private set; }

        public DXModelDefinition(DXElementDefinition ownSingleItem)
        {
            this.OwnSingleItem = ownSingleItem;
            this.SingleFragmentDefinitions = new List<DXElementDefinition>();
            this.MultiFragmentDefinitions = new List<DXElementDefinition>();
        }


        public void AppendToSingleItemDefinitions(DXElementDefinition item)
        {
            var existingFragmentDefinition = this.SingleFragmentDefinitions.SingleOrDefault(x => x.Type == item.Type);

            if (existingFragmentDefinition != null)
            {
                throw new Exception($"ASQLFragmentDefinition with type {item.Type} is existing already in ASQLModelDefinition in singlefragments list.");
            }

            this.SingleFragmentDefinitions = this.SingleFragmentDefinitions.Append(item);
        }

        public void AppendToSingleItemDefinitions(IEnumerable<DXElementDefinition> items)
        {
            foreach (var item in items)
            {
                var existingFragmentDefinition = this.SingleFragmentDefinitions.SingleOrDefault(x => x.Type == item.Type);

                if (existingFragmentDefinition != null)
                {
                    throw new Exception($"ASQLFragmentDefinition with type {item.Type} is existing already in ASQLModelDefinition in singlefragments list.");
                }
            }

            foreach (var item in items)
            {
                this.SingleFragmentDefinitions = this.SingleFragmentDefinitions.Append(item);
            }
        }

        public void AppendToMultiItemDefinitions(DXElementDefinition item)
        {
            var existingFragmentDefinition = this.MultiFragmentDefinitions.SingleOrDefault(x => x.Type == item.Type);

            if (existingFragmentDefinition != null)
            {
                throw new Exception($"ASQLFragmentDefinition with type {item.Type} is existing already in ASQLModelDefinition in multifragments list.");
            }

            this.MultiFragmentDefinitions = this.MultiFragmentDefinitions.Append(item);
        }

        public void AppendToMultiItemDefinitions(IEnumerable<DXElementDefinition> items)
        {
            foreach (var item in items)
            {
                var existingFragmentDefinition = this.MultiFragmentDefinitions.SingleOrDefault(x => x.Type == item.Type);

                if (existingFragmentDefinition != null)
                {
                    throw new Exception($"ASQLFragmentDefinition with type {item.Type} is existing already in ASQLModelDefinition in multifragments list.");
                }
            }

            foreach (var item in items)
            {
                this.MultiFragmentDefinitions = this.MultiFragmentDefinitions.Append(item);
            }
        }

        public DXModelDefinition DeepClone()
        {
            var clone = new DXModelDefinition(this.OwnSingleItem);

            var singleFragmentDefinitionClones = this.SingleFragmentDefinitions.Select(x => x.DeepClone());
            var multiFragmentDefinitionClones = this.MultiFragmentDefinitions.Select(x => x.DeepClone());

            clone.AppendToSingleItemDefinitions(singleFragmentDefinitionClones);
            clone.AppendToMultiItemDefinitions(multiFragmentDefinitionClones);

            return clone;
        }

        public static DXModelDefinition BuildModelDefinition(
            DXUnitDefinitionUnit mainEntity,
            IEnumerable<DXElementDefinitionUnit> relatedSingleMandatoryBlocks,
            IEnumerable<DXElementDefinitionUnit> relatedSingleOptionalBlocks,
            IEnumerable<DXElementDefinitionUnit> relatedMultiMandatoryBlocks,
            IEnumerable<DXElementDefinitionUnit> relatedMultiOptionalBlocks)
        {
            if (mainEntity == null)
                return null;

            var ownBlockDefinition = new DXElementDefinition(mainEntity.DXUnitDefinitionMainElement.Name, mainEntity.DXUnitDefinitionMainElement.Name);

            var props = mainEntity.DXColumnDefinitionElement.Announced?.Select(x => new DXPropertyDefinition(x.Name, new DXColumnAttribute(x.Name)));

            ownBlockDefinition.AddPropertyDefinitions(props);

            var singleBlocks = new List<DXElementDefinitionUnit>();
            var multiBlocks = new List<DXElementDefinitionUnit>();

            var dxModel = new DXModelDefinition(ownBlockDefinition);

            if (relatedSingleMandatoryBlocks != null)
            {
                singleBlocks.AddRange(relatedSingleMandatoryBlocks);
            }

            if (relatedSingleOptionalBlocks != null)
            {
                singleBlocks.AddRange(relatedSingleOptionalBlocks);
            }

            if (relatedMultiMandatoryBlocks != null)
            {
                multiBlocks.AddRange(relatedMultiMandatoryBlocks);
            }

            if (relatedMultiOptionalBlocks != null)
            {
                multiBlocks.AddRange(relatedMultiOptionalBlocks);
            }

            if (singleBlocks.Count > 0)
            {
                dxModel.SingleFragmentDefinitions = singleBlocks.Select(x => ConvertToBlockDefinition(x)).ToList();
            }

            if (multiBlocks.Count > 0)
            {
                dxModel.MultiFragmentDefinitions = multiBlocks.Select(x => ConvertToBlockDefinition(x)).ToList();
            }

            return dxModel;
        }

        private static DXElementDefinition ConvertToBlockDefinition(DXElementDefinitionUnit block)
        {
            var props = block.DXColumnDefinitionElement.Announced
                           .Select(y => new DXPropertyDefinition(y.Name, new DXColumnAttribute(y.Name)));

            var singleFragmentDefinition = new DXElementDefinition(block.DXUnitDefinitionMainElement.Name, block.DXUnitDefinitionMainElement.Name);

            singleFragmentDefinition.AddPropertyDefinitions(props);

            return singleFragmentDefinition;
        }
    }
}