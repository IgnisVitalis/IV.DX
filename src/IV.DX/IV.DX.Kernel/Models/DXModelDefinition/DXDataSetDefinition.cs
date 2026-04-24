namespace IV.DX.Kernel.Models
{
    internal class DXDataSetDefinition
    {
        public DXMainTableDefinition MainElement { get; private set; }
        public HashSet<DXTableDefinition> SingleFragmentDefinitions { get; private set; }
        public HashSet<DXTableDefinition> MultiFragmentDefinitions { get; private set; }

        public DXDataSetDefinition(DXMainTableDefinition mainElement)
        {
            MainElement = mainElement;
            SingleFragmentDefinitions = new HashSet<DXTableDefinition>();
            MultiFragmentDefinitions = new HashSet<DXTableDefinition>();
        }


        public void AddToSingleItemDefinitions(DXTableDefinition item)
        {
            var existingFragmentDefinition = SingleFragmentDefinitions.SingleOrDefault(x => x.Type == item.Type);

            if (existingFragmentDefinition != null)
            {
                throw new Exception($"DXElementDefinition with type {item.Type} is existing already in ASQLModelDefinition in singlefragments list.");
            }

            SingleFragmentDefinitions.Add(item);
        }

        public void AddToSingleItemDefinitions(IEnumerable<DXTableDefinition> items)
        {
            foreach (var item in items)
            {
                var existingFragmentDefinition = SingleFragmentDefinitions.SingleOrDefault(x => x.Type == item.Type);

                if (existingFragmentDefinition != null)
                {
                    throw new Exception($"DXElementDefinition with type {item.Type} is existing already in ASQLModelDefinition in singlefragments list.");
                }
            }

            foreach (var item in items)
            {
                SingleFragmentDefinitions.Add(item);
            }
        }

        public void AddToMultiItemDefinitions(DXTableDefinition item)
        {
            var existingFragmentDefinition = MultiFragmentDefinitions.SingleOrDefault(x => x.Type == item.Type);

            if (existingFragmentDefinition != null)
            {
                throw new Exception($"DXElementDefinition with type {item.Type} is existing already in ASQLModelDefinition in multifragments list.");
            }

            MultiFragmentDefinitions.Add(item);
        }

        public void AddToMultiItemDefinitions(IEnumerable<DXTableDefinition> items)
        {
            foreach (var item in items)
            {
                var existingFragmentDefinition = MultiFragmentDefinitions.SingleOrDefault(x => x.Type == item.Type);

                if (existingFragmentDefinition != null)
                {
                    throw new Exception($"DXElementDefinition with type {item.Type} is existing already in ASQLModelDefinition in multifragments list.");
                }
            }

            foreach (var item in items)
            {
               MultiFragmentDefinitions.Add(item);
            }
        }

        public DXDataSetDefinition DeepClone()
        {
            var clone = new DXDataSetDefinition(MainElement);

            var singleFragmentDefinitionClones = SingleFragmentDefinitions.Select(x => x.DeepClone());
            var multiFragmentDefinitionClones = MultiFragmentDefinitions.Select(x => x.DeepClone());

            clone.AddToSingleItemDefinitions(singleFragmentDefinitionClones);
            clone.AddToMultiItemDefinitions(multiFragmentDefinitionClones);

            return clone;
        }      
    }
}