using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    public class ESQLModelDefinition
    {
        public ESQLBlockDefinition OwnSingleItem { get; private set; }
        public IEnumerable<ESQLBlockDefinition> SingleFragmentDefinitions { get; private set; }
        public IEnumerable<ESQLBlockDefinition> MultiFragmentDefinitions { get; private set; }

        public ESQLModelDefinition(ESQLBlockDefinition ownSingleItem)
        {
            this.OwnSingleItem = ownSingleItem;
            this.SingleFragmentDefinitions = new List<ESQLBlockDefinition>();
            this.MultiFragmentDefinitions = new List<ESQLBlockDefinition>();
        }


        public void AppendToSingleItemDefinitions(ESQLBlockDefinition item)
        {
            var existingFragmentDefinition = this.SingleFragmentDefinitions.SingleOrDefault(x => x.Type == item.Type);

            if (existingFragmentDefinition != null)
            {
                throw new Exception($"ASQLFragmentDefinition with type {item.Type} is existing already in ASQLModelDefinition in singlefragments list.");
            }

            this.SingleFragmentDefinitions = this.SingleFragmentDefinitions.Append(item);
        }

        public void AppendToSingleItemDefinitions(IEnumerable<ESQLBlockDefinition> items)
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

        public void AppendToMultiItemDefinitions(ESQLBlockDefinition item)
        {
            var existingFragmentDefinition = this.MultiFragmentDefinitions.SingleOrDefault(x => x.Type == item.Type);

            if (existingFragmentDefinition != null)
            {
                throw new Exception($"ASQLFragmentDefinition with type {item.Type} is existing already in ASQLModelDefinition in multifragments list.");
            }

            this.MultiFragmentDefinitions = this.MultiFragmentDefinitions.Append(item);
        }

        public void AppendToMultiItemDefinitions(IEnumerable<ESQLBlockDefinition> items)
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

        public ESQLModelDefinition DeepClone()
        {
            var clone = new ESQLModelDefinition(this.OwnSingleItem);

            var singleFragmentDefinitionClones = this.SingleFragmentDefinitions.Select(x => x.DeepClone());
            var multiFragmentDefinitionClones = this.MultiFragmentDefinitions.Select(x => x.DeepClone());

            clone.AppendToSingleItemDefinitions(singleFragmentDefinitionClones);
            clone.AppendToMultiItemDefinitions(multiFragmentDefinitionClones);

            return clone;
        }

        public static ESQLModelDefinition BuildModelDefinition(
            DPEntityDescObject mainEntity,
            IEnumerable<DPBlockDescObject> relatedSingleMandatoryBlocks,
            IEnumerable<DPBlockDescObject> relatedSingleOptionalBlocks,
            IEnumerable<DPBlockDescObject> relatedMultiMandatoryBlocks,
            IEnumerable<DPBlockDescObject> relatedMultiOptionalBlocks)
        {
            if (mainEntity == null)
                return null;

            var ownBlockDefinition = new ESQLBlockDefinition(mainEntity.DPObjectDescGenBlock.Name, mainEntity.DPObjectDescGenBlock.Name);

            var props = mainEntity.DPColumnDescBlock.Announced?.Select(x => new ESQLPropertyDefinition(x.Name, new ESQLColumnDefinitionAttribute(x.Name)));

            ownBlockDefinition.AddPropertyDefinitions(props);

            var singleBlocks = new List<DPBlockDescObject>();
            var multiBlocks = new List<DPBlockDescObject>();

            var esqlModel = new ESQLModelDefinition(ownBlockDefinition);

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
                esqlModel.SingleFragmentDefinitions = singleBlocks.Select(x => ConvertToBlockDefinition(x)).ToList();
            }

            if (multiBlocks.Count > 0)
            {
                esqlModel.MultiFragmentDefinitions = multiBlocks.Select(x => ConvertToBlockDefinition(x)).ToList();
            }

            return esqlModel;
        }

        private static ESQLBlockDefinition ConvertToBlockDefinition(DPBlockDescObject block)
        {
            var props = block.DPColumnDescBlock.Announced
                           .Select(y => new ESQLPropertyDefinition(y.Name, new ESQLColumnDefinitionAttribute(y.Name)));

            var singleFragmentDefinition = new ESQLBlockDefinition(block.DPObjectDescGenBlock.Name, block.DPObjectDescGenBlock.Name);

            singleFragmentDefinition.AddPropertyDefinitions(props);

            return singleFragmentDefinition;
        }
    }
}