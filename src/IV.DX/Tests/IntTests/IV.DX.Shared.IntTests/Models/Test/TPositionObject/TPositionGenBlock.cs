using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;

namespace IV.DataProvider.Persistence.Shared.IntTests.Models.Test
{
    [DXElement("TPositionMainElement")]
    public class TPositionMainElement : DXElement
    {
        [DXColumn("Name")]
        public string Name { get; set; }
    }
}