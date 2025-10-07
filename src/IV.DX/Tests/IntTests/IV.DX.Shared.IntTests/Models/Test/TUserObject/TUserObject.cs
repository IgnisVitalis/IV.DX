using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;

namespace IV.DataProvider.Persistence.Shared.IntTests.Models.Test
{
    [DXUnit("TUserUnit")]
    public class TUserUnit : DXUnit
    {
        public TUserMainElement TUserMainElement { get; set; }
    }
}