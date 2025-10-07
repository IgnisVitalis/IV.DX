using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;

namespace IV.DataProvider.Persistence.Shared.IntTests.Models.Test
{
    [ESQLObjectDefinition("TUserObject")]
    public class TUserObject : ESQLObject
    {
        public TUserGenBlock TUserGenBlock { get; set; }
    }
}