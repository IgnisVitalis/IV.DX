using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXPublicAccessUnit")]
    public class DXPublicAccessUnit : DXUnit
    {
        [DXColumn("DXUnitDefinition", "DXUnitDefinition", DXLoadingType.Base)]
        public Guid DXUnitDefinition { get; set; }

        [DXColumn("PublicDXUnitID")]
        public Guid PublicDXUnitID { get; set; }
    }
}