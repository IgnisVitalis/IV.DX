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
            IEnumerable<DXElementDefinitionUnit> relatedSingleMandatoryDXElements,
            IEnumerable<DXElementDefinitionUnit> relatedSingleOptionalDXElements,
            IEnumerable<DXElementDefinitionUnit> relatedMultiMandatoryDXElements,
            IEnumerable<DXElementDefinitionUnit> relatedMultiOptionalDXElements)
        {
            if (mainEntity == null)
                return null;

            var ownDXElementDefinition = new DXElementDefinition(mainEntity.DXUnitDefinitionMainElement.Name, mainEntity.DXUnitDefinitionMainElement.Name);

            var props = mainEntity.DXColumnDefinitionElement.Announced?.Select(x => new DXPropertyDefinition(x.Name, new DXColumnAttribute(x.Name)));

            ownDXElementDefinition.AddPropertyDefinitions(props);

            var singleDXElements = new List<DXElementDefinitionUnit>();
            var multiDXElements = new List<DXElementDefinitionUnit>();

            var dxModel = new DXModelDefinition(ownDXElementDefinition);

            if (relatedSingleMandatoryDXElements != null)
            {
                singleDXElements.AddRange(relatedSingleMandatoryDXElements);
            }

            if (relatedSingleOptionalDXElements != null)
            {
                singleDXElements.AddRange(relatedSingleOptionalDXElements);
            }

            if (relatedMultiMandatoryDXElements != null)
            {
                multiDXElements.AddRange(relatedMultiMandatoryDXElements);
            }

            if (relatedMultiOptionalDXElements != null)
            {
                multiDXElements.AddRange(relatedMultiOptionalDXElements);
            }

            if (singleDXElements.Count > 0)
            {
                dxModel.SingleFragmentDefinitions = singleDXElements.Select(x => ConvertToDXElementDefinition(x)).ToList();
            }

            if (multiDXElements.Count > 0)
            {
                dxModel.MultiFragmentDefinitions = multiDXElements.Select(x => ConvertToDXElementDefinition(x)).ToList();
            }

            return dxModel;
        }

        private static DXElementDefinition ConvertToDXElementDefinition(DXElementDefinitionUnit dxElement)
        {
            var props = dxElement.DXColumnDefinitionElement.Announced
                           .Select(y => new DXPropertyDefinition(y.Name, new DXColumnAttribute(y.Name)));

            var singleFragmentDefinition = new DXElementDefinition(dxElement.DXUnitDefinitionMainElement.Name, dxElement.DXUnitDefinitionMainElement.Name);

            singleFragmentDefinition.AddPropertyDefinitions(props);

            return singleFragmentDefinition;
        }
    }
}