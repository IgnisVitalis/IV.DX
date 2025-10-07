using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;

namespace IV.DX.Kernel.Models
{
    [DXElement("DXColumnDefinitionElement")]
    public class DXColumnDefinitionElement : ESQLBlock
    {
        [DXColumn("Name")]
        public string Name { get; set; }
        [DXColumn("ColumnType")]
        public DXColumnTypeEnum ColumnType { get; set; }
        [DXColumn("Length")]
        public int? Length { get; set; }
        [DXColumn("Precision")]
        public int? Precision { get; set; }
        [DXColumn("Scale")]
        public int? Scale { get; set; }
        [DXColumn("AllowNull")]
        public bool AllowNull { get; set; }
        [DXColumn("DefaultValue")]
        public string DefaultValue { get; set; }
        [DXColumn("EnumKey")]
        public Guid? EnumKey { get; set; }
        [DXColumn("EnumType")]
        public Guid? EnumType { get; set; }
    }
}