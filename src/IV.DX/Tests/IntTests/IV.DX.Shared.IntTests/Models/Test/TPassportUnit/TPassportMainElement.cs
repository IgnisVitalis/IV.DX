using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;

namespace IV.DX.Shared.IntTests.Models.Test
{
    [DXElement("TPassportMainElement")]
    public class TPassportMainElement : DXElement
    {
        [DXColumn("SerialNumber")]
        public string SerialNumber { get; set; }
    }
}