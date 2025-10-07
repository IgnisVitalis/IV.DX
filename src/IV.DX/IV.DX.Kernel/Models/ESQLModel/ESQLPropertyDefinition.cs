using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    public class ESQLPropertyDefinition
    {
        public string Name { get; set; }
        public DXColumnAttribute ColumnDefinition { get; set; }

        private ESQLPropertyDefinition()
        {

        }

        public ESQLPropertyDefinition(string name, DXColumnAttribute columnDefinition)
        {
            this.Name = name;
            this.ColumnDefinition = columnDefinition;
        }

        public ESQLPropertyDefinition DeepClone()
        {
            var clone = new ESQLPropertyDefinition()
            {
                Name = this.Name,
                ColumnDefinition = this.ColumnDefinition.DeepClone()
            };

            return clone;
        }

        public bool DeepEquals(ESQLPropertyDefinition columnDefinition)
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