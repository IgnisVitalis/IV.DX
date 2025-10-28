using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    internal class DXPropertyDefinition
    {
        public string Name { get; set; }
        public DXColumnAttribute ColumnDefinition { get; set; }

        private DXPropertyDefinition()
        {

        }

        public DXPropertyDefinition(string name, DXColumnAttribute columnDefinition)
        {
            Name = name;
            ColumnDefinition = columnDefinition;
        }

        public DXPropertyDefinition DeepClone()
        {
            var clone = new DXPropertyDefinition()
            {
                Name = Name,
                ColumnDefinition = ColumnDefinition.DeepClone()
            };

            return clone;
        }

        public bool DeepEquals(DXPropertyDefinition columnDefinition)
        {
            if (columnDefinition == null)
                return false;

            if (Name != columnDefinition.Name)
                return false;

            if (!ColumnDefinition.DeepEquals(columnDefinition.ColumnDefinition))
                return false;

            return true;
        }
    }
}