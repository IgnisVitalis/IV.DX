using IV.DX.Kernel.Attributes;
using System.Collections;

namespace IV.DX.Kernel.Models
{
    internal class DXTableDefinition : IEnumerable<DXColumnDefinition>
    {
        private readonly List<DXColumnDefinition> _items;
        public string Name { get; }
        public string Type { get; }
        public string DXUnitType { get; }
        public bool IsRequired { get; }

        public DXTableDefinition(string dxUnitType, string dxElementType, string name, bool isRequired)
        {
            this.Name = name;
            this.Type = dxElementType;
            this.IsRequired = isRequired;
            this.DXUnitType = dxUnitType;

            _items = new List<DXColumnDefinition>()
            {
                new DXColumnDefinition(Constants.ID, new DXColumnAttribute(Constants.ID)),
                new DXColumnDefinition(Constants.TimeStamp, new DXColumnAttribute(Constants.TimeStamp)),
                new DXColumnDefinition(Constants.DXCustomUnitID(dxUnitType), new DXColumnAttribute(Constants.DXCustomUnitID(dxUnitType)))
            };
        }

        public void AddPropertyDefinition(DXColumnDefinition item)
        {
            if (item.Name == Constants.ID
                || item.Name == Constants.TimeStamp
                || item.Name == Constants.DXCustomUnitID(this.DXUnitType)
                )
                return;

            if (GetPropertyDefinitionByName(item.Name) != null)
                throw new Exception($"ASQLPropertyDefinition with Name {item.Name} is already existing.");

            _items.Add(item);
        }

        public void AddPropertyDefinitions(IEnumerable<DXColumnDefinition> items)
        {
            if (items == null || items.Count() == 0)
                return;

            foreach (var item in items)
            {
                AddPropertyDefinition(item);
            }
        }

        public DXColumnDefinition? GetPropertyDefinitionByName(string name)
        {
            var item = _items.SingleOrDefault(x => x.Name == name);

            return item;
        }

        public DXTableDefinition DeepClone()
        {
            var clone = new DXTableDefinition(this.DXUnitType, Type, Name, this.IsRequired);
            clone.AddPropertyDefinitions(_items);

            return clone;
        }

        public IEnumerator<DXColumnDefinition> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public IDictionary<string, string> GetColumns()
        {
            return this._items.ToDictionary(x => x.ColumnAttribute.Name, x => x.ColumnAttribute.DXExpression).ToDictionary();
        }
    }
}
