using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using System;

namespace IV.DataProvider.Persistence.Shared.IntTests.Models.Test
{
    [DXUnit("TPositionObject")]
    public class TPositionObject : ESQLObject
    {
        [DXColumn("User", "User", DXLoadingType.Base)]
        public Guid? User { get; set; }
        public TPositionGenBlock TPositionGenBlock { get; set; }
    }
}