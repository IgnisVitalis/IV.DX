using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;

namespace IV.DX.Kernel.Models
{
    [ESQLBlockDefinition("DPColumnDescBlock")]
    public class DPColumnDescBlock : ESQLBlock
    {
        [ESQLColumnDefinition("Name")]
        public string Name { get; set; }
        [ESQLColumnDefinition("ColumnType")]
        public DPColumnTypeEnum ColumnType { get; set; }
        [ESQLColumnDefinition("Length")]
        public int? Length { get; set; }
        [ESQLColumnDefinition("Precision")]
        public int? Precision { get; set; }
        [ESQLColumnDefinition("Scale")]
        public int? Scale { get; set; }
        [ESQLColumnDefinition("AllowNull")]
        public bool AllowNull { get; set; }
        [ESQLColumnDefinition("DefaultValue")]
        public string DefaultValue { get; set; }
        [ESQLColumnDefinition("EnumKey")]
        public Guid? EnumKey { get; set; }
        [ESQLColumnDefinition("EnumType")]
        public Guid? EnumType { get; set; }
    }
}