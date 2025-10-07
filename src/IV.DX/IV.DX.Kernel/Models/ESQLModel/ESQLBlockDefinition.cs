using System.Collections;

namespace IV.DX.Kernel.Models
{
    public class ESQLBlockDefinition : IEnumerable<ESQLPropertyDefinition>
    {
        private readonly List<ESQLPropertyDefinition> _items;

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

        public ESQLBlockDefinition(string type, string name)
        {
            this.Name = name;
            this.Type = type;
            this._items = new List<ESQLPropertyDefinition>();
        }

        public void AddPropertyDefinition(ESQLPropertyDefinition item)
        {
            if (this.GetPropertyDefinitionByName(item.Name) != null)
                throw new Exception($"ASQLPropertyDefinition with Name {item.Name} is already existing.");

            this._items.Add(item);
        }

        public void AddPropertyDefinitions(IEnumerable<ESQLPropertyDefinition> items)
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

        public ESQLPropertyDefinition GetPropertyDefinitionByName(string name)
        {
            var item = this._items.SingleOrDefault(x => x.Name == name);

            return item;
        }

        public ESQLBlockDefinition DeepClone()
        {
            var clone = new ESQLBlockDefinition(this.Type, this.Name);
            clone.AddPropertyDefinitions(this._items);

            return clone;
        }

        public IEnumerator<ESQLPropertyDefinition> GetEnumerator()
        {
            return this._items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._items.GetEnumerator();
        }
    }
}