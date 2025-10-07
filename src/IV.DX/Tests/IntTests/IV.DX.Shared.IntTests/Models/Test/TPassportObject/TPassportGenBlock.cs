using IV.DataProvider.Persistence.Contracts.Models;
using IV.DX.Contracts.Common.Attributes;

namespace IV.DataProvider.Persistence.Shared.IntTests.Models.Test
{
    [ESQLBlockDefinition("TPassportGenBlock")]
    public class TPassportGenBlock : ESQLBlock
    {
        [ESQLColumnDefinition("SerialNumber")]
        public string SerialNumber { get; set; }
    }
}