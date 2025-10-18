using IV.DX.Kernel.Enums;

namespace IV.DX.Application.Contracts.Models
{
    public class DXUnitStructure
    {
        public string Name { get; set; }
        public List<DXElementDefinion> SingleItemMandatory { get; set; }
        public List<DXElementDefinion> SingleItemOptional { get; set; }
        public List<DXElementDefinion> MultiItemsMandatory { get; set; }
        public List<DXElementDefinion> MultiItemsOptional { get; set; }
    }

    public class DXElementDefinion
    {
        public string Name { get; set; }

        public IEnumerable<DXColumnDefinition> Columns { get; set; }
    }

    public class DXColumnDefinition
    {
        public string Name { get; set; }
        public DXColumnTypeEnum ColumnType { get; set; }
        public int? Length { get; set; }
        public int? Precision { get; set; }
        public int? Scale { get; set; }
        public bool AllowNull { get; set; }
        public string DefaultValue { get; set; }
        public IDictionary<int, string> EnumValues { get; set; }
    }
}