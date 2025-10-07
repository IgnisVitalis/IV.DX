using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;

namespace IV.DataProvider.Persistence.Shared.IntTests.Models.Test
{
    [DXElement("TBookChapterElement")]
    public class TBookChapterElement : DXElement
    {
        [DXColumn("Text")]
        public string Text { get; set; }
        [DXColumn("Number")]
        public int Number { get; set; }
    }
}