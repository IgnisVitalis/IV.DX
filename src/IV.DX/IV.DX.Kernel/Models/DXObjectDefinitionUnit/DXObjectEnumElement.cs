using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXElement("DXObjectEnumElement")]
    public class DXObjectEnumElement : DXElement
    {
        [DXColumn("EnumKey")]
        public Guid EnumKey { get; set; }
        [DXColumn("EnumType")]
        public Guid EnumType { get; set; }
        [DXColumn("Name")]
        public string Name { get; set; }
    }
}