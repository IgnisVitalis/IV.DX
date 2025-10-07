using IV.DataProvider.Persistence.Contracts.Models;
using IV.DX.Contracts.Common.Attributes;
using System;

namespace IV.DataProvider.Persistence.Shared.IntTests.Models.Test
{
    [ESQLBlockDefinition("TUserGenBlock")]
    public class TUserGenBlock : ESQLBlock
    {
        [ESQLColumnDefinition("Name")]
        public string Name { get; set; }
        [ESQLColumnDefinition("Surname")]
        public string Surname { get; set; }
        [ESQLColumnDefinition("Birth")]
        public DateTime Birth { get; set; }
    }
}