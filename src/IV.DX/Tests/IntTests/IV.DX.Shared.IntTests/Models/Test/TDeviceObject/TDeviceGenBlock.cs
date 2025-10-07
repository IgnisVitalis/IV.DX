using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;
using System;

namespace IV.DataProvider.Persistence.Shared.IntTests.Models.Test
{
    [ESQLBlockDefinition("TDeviceGenBlock")]
    public class TDeviceGenBlock : ESQLBlock
    {
        [ESQLColumnDefinition("Model")]
        public string Model { get; set; }
        [ESQLColumnDefinition("UUID")]
        public Guid UUID { get; set; }
    }
}