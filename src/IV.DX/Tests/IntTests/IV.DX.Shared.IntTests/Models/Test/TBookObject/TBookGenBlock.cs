using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;

namespace IV.DataProvider.Persistence.Shared.IntTests.Models.Test
{
    [ESQLBlockDefinition("TBookChapterBlock")]
    public class TBookChapterBlock : ESQLBlock
    {
        [ESQLColumnDefinition("Text")]
        public string Text { get; set; }
        [ESQLColumnDefinition("Number")]
        public int Number { get; set; }
    }
}