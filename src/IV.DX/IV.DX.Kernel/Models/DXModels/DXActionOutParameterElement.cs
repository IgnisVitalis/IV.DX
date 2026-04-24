using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXElement("DXActionOutParameterElement")]
    public class DXActionOutParameterElement : DXElement
    {
        [DXColumn("Key")]
        public string Key { get; set; } = null!;

        [DXColumn("Type")]
        public DXActionParameterTypeEnum Type { get; set; }

        [DXColumn("Required")]
        public bool Required { get; set; }

        [DXColumn("IsMulti")]
        public bool IsMulti { get; set; }
    }
}
