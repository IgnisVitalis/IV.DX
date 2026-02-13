using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;

namespace IV.DX.Kernel.Models
{
    [DXElement("DXRoleElement")]
    public class DXRoleElement : DXElement
    {
        [DXColumn("Role", "E2U(Role).ID", DXLoadingType.Base)]
        public Guid Role { get; set; }
    }
}
