using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;

namespace IV.DataProvider.Persistence.Shared.IntTests.Models.Test
{
    [DXElement("TPositionGenBlock")]
    public class TPositionGenBlock : ESQLBlock
    {
        [DXColumn("Name")]
        public string Name { get; set; }
    }
}