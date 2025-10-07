using IV.DataProvider.Persistence.Contracts.Models;
using IV.DX.Contracts.Common.Attributes;

namespace IV.DX.Contracts.Common.Models
{
    [ESQLBlockDefinition("DPColumnsUniqueBlock")]
    public class DPColumnsUniqueBlock : ESQLBlock
    {
        [ESQLColumnDefinition("Columns")]
        public string Columns { get; set; }
    }
}