using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using System;

namespace IV.DataProvider.Persistence.Shared.IntTests.Models.Test
{
    [ESQLObjectDefinition("TPositionObject")]
    public class TPositionObject : ESQLObject
    {
        [ESQLColumnDefinition("User", "User", TypeOfEntityLoading.Base)]
        public Guid? User { get; set; }
        public TPositionGenBlock TPositionGenBlock { get; set; }
    }
}