using IV.DataProvider.Persistence.Contracts.Models;
using IV.DX.Contracts.Common.Attributes;
using IV.DX.Contracts.Common.Models;

namespace IV.DataProvider.Persistence.Shared.IntTests.Models.Test
{
    [ESQLBlockDefinition("TDocumentGenBlock")]
    public class TDocumentGenBlock : ESQLBlock
    {
        [ESQLColumnDefinition("Name")]
        public string Name { get; set; }
    }
}