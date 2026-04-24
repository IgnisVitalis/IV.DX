using IV.DX.Kernel.Attributes;
using System.Collections;

namespace IV.DX.Kernel.Models
{
    internal class DXMainTableDefinition : IEnumerable<DXColumnDefinition>
    {
        private readonly List<DXColumnDefinition> _items;
        public string Name { get; }
        public string DXUnitType { get; }
        public string? DXTitleExpression { get; set; }

        public DXMainTableDefinition(string dxUnitType, string name)
        {
            this.Name = name;
            this.DXUnitType = dxUnitType;

            _items = new List<DXColumnDefinition>()
            {
                new DXColumnDefinition(Constants.ID, new DXColumnAttribute(Constants.ID)),
                new DXColumnDefinition(Constants.TimeStamp, new DXColumnAttribute(Constants.TimeStamp)),
            };
        }

        public void AddPropertyDefinition(DXColumnDefinition item)
        {
            if (item.Name == Constants.ID
                || item.Name == Constants.TimeStamp
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

        public DXMainTableDefinition DeepClone()
        {
            var clone = new DXMainTableDefinition(this.DXUnitType, this.Name);
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
            var columns = this._items.ToDictionary(x => x.ColumnAttribute.Name, x => x.ColumnAttribute.DXExpression);
            if (!string.IsNullOrEmpty(DXTitleExpression))
                columns[Constants.DXTitle] = DXTitleExpression;
            return columns;
        }
    }
}
