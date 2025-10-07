using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    public class DXPropertyDefinition
    {
        public string Name { get; set; }
        public DXColumnAttribute ColumnDefinition { get; set; }

        private DXPropertyDefinition()
        {

        }

        public DXPropertyDefinition(string name, DXColumnAttribute columnDefinition)
        {
            this.Name = name;
            this.ColumnDefinition = columnDefinition;
        }

        public DXPropertyDefinition DeepClone()
        {
            var clone = new DXPropertyDefinition()
            {
                Name = this.Name,
                ColumnDefinition = this.ColumnDefinition.DeepClone()
            };

            return clone;
        }

        public bool DeepEquals(DXPropertyDefinition columnDefinition)
        {
            if (columnDefinition == null)
                return false;

            if (this.Name != columnDefinition.Name)
                return false;

            if (!this.ColumnDefinition.DeepEquals(columnDefinition.ColumnDefinition))
                return false;

            return true;
        }
    }
}