using IV.DX.Kernel.Attributes;
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
                return _name;
            }
            private set
            {
                _name = value?.Trim();
            }
        }

        private string _type;

        public string Type
        {
            get
            {
                return _type;
            }
            private set
            {
                _type = value?.Trim();
            }
        }

        public bool IsRequired { get; }

        public DXElementDefinition(string type, string name, bool isRequired)
        {
            this.Name = name;
            this.Type = type;
            this.IsRequired = isRequired;

            _items = new List<DXPropertyDefinition>()
            {
                new DXPropertyDefinition(Constants.ID, new DXColumnAttribute(Constants.ID)),
                new DXPropertyDefinition(Constants.TimeStamp, new DXColumnAttribute(Constants.TimeStamp))
            };
        }

        public void AddPropertyDefinition(DXPropertyDefinition item)
        {
            if (item.Name == Constants.SystemPropertyTypeName
                || item.Name == Constants.ID
                || item.Name == Constants.TimeStamp
                )
                return;

            if (GetPropertyDefinitionByName(item.Name) != null)
                throw new Exception($"ASQLPropertyDefinition with Name {item.Name} is already existing.");

            _items.Add(item);
        }

        public void AddPropertyDefinitions(IEnumerable<DXPropertyDefinition> items)
        {
            if (items == null || items.Count() == 0)
                return;

            foreach (var item in items)
            {
                AddPropertyDefinition(item);
            }
        }

        public DXPropertyDefinition GetPropertyDefinitionByName(string name)
        {
            var item = _items.SingleOrDefault(x => x.Name == name);

            return item;
        }

        public DXElementDefinition DeepClone()
        {
            var clone = new DXElementDefinition(Type, Name, this.IsRequired);
            clone.AddPropertyDefinitions(_items);

            return clone;
        }

        public IEnumerator<DXPropertyDefinition> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public IDictionary<string, string> GetColumns()
        {
            return this._items.ToDictionary(x => x.ColumnDefinition.Name, x => x.ColumnDefinition.DXExpression).ToDictionary();
        }
    }
}