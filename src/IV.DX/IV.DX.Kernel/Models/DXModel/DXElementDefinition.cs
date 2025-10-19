using System.Collections;

namespace IV.DX.Kernel.Models
{
    internal class DXElementDefinition : IEnumerable<DXPropertyDefinition>
    {
        private readonly List<DXPropertyDefinition> _items;

        private string _name;
        public string Name
        {
            get
            {
                return this._name;
            }
            private set
            {
                this._name = value?.Trim();
            }
        }

        private string _type;

        public string Type
        {
            get
            {
                return this._type;
            }
            private set
            {
                this._type = value?.Trim();
            }
        }

        public DXElementDefinition(string type, string name)
        {
            this.Name = name;
            this.Type = type;
            this._items = new List<DXPropertyDefinition>();
        }

        public void AddPropertyDefinition(DXPropertyDefinition item)
        {
            if (this.GetPropertyDefinitionByName(item.Name) != null)
                throw new Exception($"ASQLPropertyDefinition with Name {item.Name} is already existing.");

            this._items.Add(item);
        }

        public void AddPropertyDefinitions(IEnumerable<DXPropertyDefinition> items)
        {
            if (items == null || items.Count() == 0)
                return;

            foreach (var item in items)
            {
                if (this.GetPropertyDefinitionByName(item.Name) != null)
                    throw new Exception($"ASQLPropertyDefinition with Name {item.Name} is already existing.");
            }

            foreach (var item in items)
            {
                this._items.Add(item);
            }
        }

        public DXPropertyDefinition GetPropertyDefinitionByName(string name)
        {
            var item = this._items.SingleOrDefault(x => x.Name == name);

            return item;
        }

        public DXElementDefinition DeepClone()
        {
            var clone = new DXElementDefinition(this.Type, this.Name);
            clone.AddPropertyDefinitions(this._items);

            return clone;
        }

        public IEnumerator<DXPropertyDefinition> GetEnumerator()
        {
            return this._items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._items.GetEnumerator();
        }
    }
}