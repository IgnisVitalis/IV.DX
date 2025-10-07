using IV.DataProvider.Persistence.Contracts.Models;
using IV.DX.Contracts.Common.Attributes;
using IV.DX.Contracts.Common.Models;

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