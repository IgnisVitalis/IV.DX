using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;

namespace IV.DataProvider.Persistence.Shared.IntTests.Models.Test
{
    [ESQLBlockDefinition("TPassportGenBlock")]
    public class TPassportGenBlock : ESQLBlock
    {
        [ESQLColumnDefinition("SerialNumber")]
        public string SerialNumber { get; set; }
    }
}