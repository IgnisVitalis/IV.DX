using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    internal class DXColumnDefinition
    {
        public string Name { get; set; }
        public DXColumnAttribute ColumnAttribute { get; set; }

        private DXColumnDefinition()
        {

        }

        public DXColumnDefinition(string name, DXColumnAttribute columnAttribute)
        {
            Name = name;
            ColumnAttribute = columnAttribute;
        }

        public DXColumnDefinition DeepClone()
        {
            var clone = new DXColumnDefinition()
            {
                Name = Name,
                ColumnAttribute = ColumnAttribute.DeepClone()
            };

            return clone;
        }

        public bool DeepEquals(DXColumnDefinition columnDefinition)
        {
            if (columnDefinition == null)
                return false;

            if (Name != columnDefinition.Name)
                return false;

            if (!ColumnAttribute.DeepEquals(columnDefinition.ColumnAttribute))
                return false;

            return true;
        }

        public override string ToString()
        {
            return $"{this.Name}; {this.ColumnAttribute.Name} AS {this.ColumnAttribute.DXExpression};";
        }
    }
}