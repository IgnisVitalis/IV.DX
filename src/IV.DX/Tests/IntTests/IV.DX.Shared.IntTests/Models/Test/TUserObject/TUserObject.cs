using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;

namespace IV.DataProvider.Persistence.Shared.IntTests.Models.Test
{
    [DXUnit("TUserObject")]
    public class TUserObject : DXUnit
    {
        public TUserGenBlock TUserGenBlock { get; set; }
    }
}