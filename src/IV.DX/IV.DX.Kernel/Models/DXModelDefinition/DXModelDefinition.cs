namespace IV.DX.Kernel.Models
{
    internal class DXModelDefinition
    {
        public DXElementDefinition MainElement { get; private set; }
        public HashSet<DXElementDefinition> SingleFragmentDefinitions { get; private set; }
        public HashSet<DXElementDefinition> MultiFragmentDefinitions { get; private set; }

        public DXModelDefinition(DXElementDefinition mainElement)
        {
            MainElement = mainElement;
            SingleFragmentDefinitions = new HashSet<DXElementDefinition>();
            MultiFragmentDefinitions = new HashSet<DXElementDefinition>();
        }


        public void AddToSingleItemDefinitions(DXElementDefinition item)
        {
            var existingFragmentDefinition = SingleFragmentDefinitions.SingleOrDefault(x => x.Type == item.Type);

            if (existingFragmentDefinition != null)
            {
                throw new Exception($"ASQLFragmentDefinition with type {item.Type} is existing already in ASQLModelDefinition in singlefragments list.");
            }

            SingleFragmentDefinitions.Add(item);
        }

        public void AddToSingleItemDefinitions(IEnumerable<DXElementDefinition> items)
        {
            foreach (var item in items)
            {
                var existingFragmentDefinition = SingleFragmentDefinitions.SingleOrDefault(x => x.Type == item.Type);

                if (existingFragmentDefinition != null)
                {
                    throw new Exception($"ASQLFragmentDefinition with type {item.Type} is existing already in ASQLModelDefinition in singlefragments list.");
                }
            }

            foreach (var item in items)
            {
                SingleFragmentDefinitions.Add(item);
            }
        }

        public void AddToMultiItemDefinitions(DXElementDefinition item)
        {
            var existingFragmentDefinition = MultiFragmentDefinitions.SingleOrDefault(x => x.Type == item.Type);

            if (existingFragmentDefinition != null)
            {
                throw new Exception($"ASQLFragmentDefinition with type {item.Type} is existing already in ASQLModelDefinition in multifragments list.");
            }

            MultiFragmentDefinitions.Add(item);
        }

        public void AddToMultiItemDefinitions(IEnumerable<DXElementDefinition> items)
        {
            foreach (var item in items)
            {
                var existingFragmentDefinition = MultiFragmentDefinitions.SingleOrDefault(x => x.Type == item.Type);

                if (existingFragmentDefinition != null)
                {
                    throw new Exception($"ASQLFragmentDefinition with type {item.Type} is existing already in ASQLModelDefinition in multifragments list.");
                }
            }

            foreach (var item in items)
            {
               MultiFragmentDefinitions.Add(item);
            }
        }

        public DXModelDefinition DeepClone()
        {
            var clone = new DXModelDefinition(MainElement);

            var singleFragmentDefinitionClones = SingleFragmentDefinitions.Select(x => x.DeepClone());
            var multiFragmentDefinitionClones = MultiFragmentDefinitions.Select(x => x.DeepClone());

            clone.AddToSingleItemDefinitions(singleFragmentDefinitionClones);
            clone.AddToMultiItemDefinitions(multiFragmentDefinitionClones);

            return clone;
        }      
    }
}