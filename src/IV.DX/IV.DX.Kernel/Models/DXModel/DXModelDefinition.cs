using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters.DXModelDefinitionConverters;

namespace IV.DX.Kernel.Models
{
    internal class DXModelDefinition
    {
        public DXElementDefinition MainElement { get; private set; }
        public HashSet<DXElementDefinition> SingleFragmentDefinitions { get; private set; }
        public HashSet<DXElementDefinition> MultiFragmentDefinitions { get; private set; }

        public DXModelDefinition(DXElementDefinition mainElement)
        {
            this.MainElement = mainElement;
            this.SingleFragmentDefinitions = new HashSet<DXElementDefinition>();
            this.MultiFragmentDefinitions = new HashSet<DXElementDefinition>();
        }


        public void AddToSingleItemDefinitions(DXElementDefinition item)
        {
            var existingFragmentDefinition = this.SingleFragmentDefinitions.SingleOrDefault(x => x.Type == item.Type);

            if (existingFragmentDefinition != null)
            {
                throw new Exception($"ASQLFragmentDefinition with type {item.Type} is existing already in ASQLModelDefinition in singlefragments list.");
            }

            this.SingleFragmentDefinitions.Add(item);
        }

        public void AddToSingleItemDefinitions(IEnumerable<DXElementDefinition> items)
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
                this.SingleFragmentDefinitions.Add(item);
            }
        }

        public void AddToMultiItemDefinitions(DXElementDefinition item)
        {
            var existingFragmentDefinition = this.MultiFragmentDefinitions.SingleOrDefault(x => x.Type == item.Type);

            if (existingFragmentDefinition != null)
            {
                throw new Exception($"ASQLFragmentDefinition with type {item.Type} is existing already in ASQLModelDefinition in multifragments list.");
            }

            this.MultiFragmentDefinitions.Add(item);
        }

        public void AddToMultiItemDefinitions(IEnumerable<DXElementDefinition> items)
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
               this.MultiFragmentDefinitions.Add(item);
            }
        }

        public DXModelDefinition DeepClone()
        {
            var clone = new DXModelDefinition(this.MainElement);

            var singleFragmentDefinitionClones = this.SingleFragmentDefinitions.Select(x => x.DeepClone());
            var multiFragmentDefinitionClones = this.MultiFragmentDefinitions.Select(x => x.DeepClone());

            clone.AddToSingleItemDefinitions(singleFragmentDefinitionClones);
            clone.AddToMultiItemDefinitions(multiFragmentDefinitionClones);

            return clone;
        }      
    }
}