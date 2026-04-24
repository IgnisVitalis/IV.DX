using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXSecurityMemberUnit")]
    public class DXSecurityMemberUnit : DXUnit
    {
        [DXColumn("Name")]
        public string Name { get; set; } = null!;

        public DXMultiElementsContainer<DXRoleElement> DXRoleElement { get; set; } = new()
        {
            Announced = new HashSet<DXRoleElement>()
        };
    }
}
