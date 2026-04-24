using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXElement("DXActionInParameterElement")]
    public class DXActionInParameterElement : DXElement
    {
        [DXColumn("Key")]
        public string Key { get; set; } = null!;

        [DXColumn("Type")]
        public DXActionParameterTypeEnum Type { get; set; }

        [DXColumn("Required")]
        public bool Required { get; set; }

        [DXColumn("IsMulti")]
        public bool IsMulti { get; set; }

        [DXColumn("DefaultValue")]
        public string? DefaultValue { get; set; }
    }
}
