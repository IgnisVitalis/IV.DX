using IV.DataProvider.Persistence.Contracts.Models;
using IV.DX.Contracts.Common.Attributes;
using IV.DX.Contracts.Common.Enums;

namespace IV.DX.Contracts.Common.Models
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