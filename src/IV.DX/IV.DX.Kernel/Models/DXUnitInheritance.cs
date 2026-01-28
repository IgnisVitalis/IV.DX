namespace IV.DX.Kernel.Models
{
    public class DXUnitInheritance
    {
        public IList<DXUnitInheritanceItem> Items { get; } = new List<DXUnitInheritanceItem>();
        public IList<DXUnitInheritanceItem> ItemsReverted { get { return this.Items.Reverse().ToList(); } }
        public void Add(DXUnitInheritanceItem item)
        {
            this.Items.Add(item);
        }
    }

    public class DXUnitInheritanceItem
    {
        public DXUnitDefinitionUnit DXUnit { get; }

        public HashSet<DXElementDefinitionUnit> SingleMandatory { get; }
        public HashSet<DXElementDefinitionUnit> SingleOptional { get; }
        public HashSet<DXElementDefinitionUnit> MultiMandatory { get; }
        public HashSet<DXElementDefinitionUnit> MultiOptional { get; }

        public HashSet<DXElementDefinitionUnit> AllDXElements
        {
            get
            {
                var result = new HashSet<DXElementDefinitionUnit>();
                result.UnionWith(SingleMandatory);
                result.UnionWith(SingleOptional);
                result.UnionWith(MultiMandatory);
                result.UnionWith(MultiOptional);

                return result;
            }
        }
      
        public DXUnitInheritanceItem(
            DXUnitDefinitionUnit dxUnit,
            HashSet<DXElementDefinitionUnit> sm,
            HashSet<DXElementDefinitionUnit> so,
            HashSet<DXElementDefinitionUnit> mm,
            HashSet<DXElementDefinitionUnit> mo)
        {
            this.SingleMandatory = sm;
            this.SingleOptional = so;
            this.MultiMandatory = mm;
            this.MultiOptional = mo;
        }

        public bool ContainsSingleMandatory(string dxElementTypeName)
        {
            return this.SingleMandatory.SingleOrDefault(x => x.Name.Equals(dxElementTypeName)) != null;
        }

        public bool ContainsSingleOptional(string dxElementTypeName)
        {
            return this.SingleOptional.SingleOrDefault(x => x.Name.Equals(dxElementTypeName)) != null;
        }

        public bool ContainsMultiMandatory(string dxElementTypeName)
        {
            return this.MultiMandatory.SingleOrDefault(x => x.Name.Equals(dxElementTypeName)) != null;
        }

        public bool ContainsMultiOptional(string dxElementTypeName)
        {
            return this.MultiOptional.SingleOrDefault(x => x.Name.Equals(dxElementTypeName)) != null;
        }
    }
}