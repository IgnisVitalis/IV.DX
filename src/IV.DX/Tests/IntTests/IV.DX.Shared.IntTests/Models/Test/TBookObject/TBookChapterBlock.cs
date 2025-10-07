using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;

namespace IV.DataProvider.Persistence.Shared.IntTests.Models.Test
{
    [DXElement("TBookGenBlock")]
    public class TBookGenBlock : ESQLBlock
    {
        [DXColumn("Name")]
        public string Name { get; set; }
    }
}