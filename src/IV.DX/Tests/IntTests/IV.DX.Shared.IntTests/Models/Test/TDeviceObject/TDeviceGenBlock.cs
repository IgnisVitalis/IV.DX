using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;
using System;

namespace IV.DataProvider.Persistence.Shared.IntTests.Models.Test
{
    [DXElement("TDeviceGenBlock")]
    public class TDeviceGenBlock : ESQLBlock
    {
        [DXColumn("Model")]
        public string Model { get; set; }
        [DXColumn("UUID")]
        public Guid UUID { get; set; }
    }
}