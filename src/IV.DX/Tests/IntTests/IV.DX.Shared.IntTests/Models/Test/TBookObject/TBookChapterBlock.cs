using IV.DataProvider.Persistence.Contracts.Models;
using IV.DX.Contracts.Common.Attributes;

namespace IV.DataProvider.Persistence.Shared.IntTests.Models.Test
{
    [ESQLBlockDefinition("TBookGenBlock")]
    public class TBookGenBlock : ESQLBlock
    {
        [ESQLColumnDefinition("Name")]
        public string Name { get; set; }
    }
}