using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;
using System;

namespace IV.DataProvider.Persistence.Shared.IntTests.Models.Test
{
    [DXElement("TUserGenBlock")]
    public class TUserGenBlock : DXElement
    {
        [DXColumn("Name")]
        public string Name { get; set; }
        [DXColumn("Surname")]
        public string Surname { get; set; }
        [DXColumn("Birth")]
        public DateTime Birth { get; set; }
    }
}