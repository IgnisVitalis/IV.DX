using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;

namespace IV.DataProvider.Persistence.Shared.IntTests.Models.Test
{
    [DXElement("TDocumentGenBlock")]
    public class TDocumentGenBlock : ESQLBlock
    {
        [DXColumn("Name")]
        public string Name { get; set; }
    }
}