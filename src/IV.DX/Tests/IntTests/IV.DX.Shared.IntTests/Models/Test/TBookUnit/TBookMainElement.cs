using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;

namespace IV.DX.Shared.IntTests.Models.Test
{
    [DXElement("TBookMainElement")]
    public class TBookMainElement : DXElement
    {
        [DXColumn("Name")]
        public string Name { get; set; }
    }
}