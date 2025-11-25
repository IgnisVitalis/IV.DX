using IV.DX.Kernel.Attributes;
using System;

namespace IV.DX.Kernel.Models
{
    [DXElement("DXUnitInheritanceElement")]
    public class DXUnitInheritanceElement : DXElement
    {
        [DXColumn("BaseDXUnit")]
        public Guid BaseDXUnit { get; set; }
    }
}